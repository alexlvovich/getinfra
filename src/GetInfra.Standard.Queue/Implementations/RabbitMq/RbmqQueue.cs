using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using GetInfra.Standard.Queue.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq
{
    public class RbmqQueue : IQueue, IDisposable
    {
        public ILogger _logger;
        private readonly QueueSettings _publisherSettings;
        private readonly QueueSettings _consumerSettings;

        private JsonSerializerSettings _consumerSerializationSettings;
        private JsonSerializerSettings _publisherSerializationSettings;
        private IConnection _consumerConn;
        private IConnection _publishConn;
        private IModel _publishChannel;
        private IModel _consumerChannel;
        private object _lockPublisher = new object();
        private object _lockSubscriber = new object();

        private ManualResetEvent waitHandle = new ManualResetEvent(false);
        public event Action<object, QMessage> MessageRecieved;
        private EventingBasicConsumer _consumer;
        private readonly object _queueLock = new object();


        public JsonSerializerSettings ConsumerSerializationSettings
        {
            get
            {
                return _consumerSerializationSettings;
            }
            set
            {
                _consumerSerializationSettings = value;
            }
        }

        public JsonSerializerSettings PublisherSerializationSettings
        {
            get
            {
                return _publisherSerializationSettings;
            }
            set
            {
                _publisherSerializationSettings = value;
            }
        }

        public QueueSettings PublisherSettings
        {
            get
            {
                return _publisherSettings;
            }
        }

        public QueueSettings ConsumerSettings
        {
            get
            {
                return _consumerSettings;
            }
        }

        #region .CTOR
        public RbmqQueue(ILoggerFactory loggerFactory, RbmqConfigurationElement settings, bool publisher = true)
        {
            _logger = loggerFactory.CreateLogger<RbmqQueue>();
            // default serialization settings
            this._publisherSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            this._consumerSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };


            if (publisher)
            {
                _publisherSettings = new QueueSettings(settings);
                if (string.IsNullOrEmpty(_publisherSettings.Queue))
                {
                    _publisherSettings.GenerateNewQueueName(true);
                    _publisherSettings.GeneratedQueueName = true;
                    _publisherSettings.Exclusive = true;
                }
                _publishConn = GetConnection(_publisherSettings);
                Initialize(_publishChannel, _publishConn, _publisherSettings);
                _publishConn.ConnectionShutdown += PublisherConnectionShutdown;
            }
            else
            {
                _consumerSettings = new QueueSettings(settings);
                if (string.IsNullOrEmpty(_consumerSettings.Queue))
                {
                    _consumerSettings.GenerateNewQueueName(false);
                    _consumerSettings.GeneratedQueueName = true;
                    _consumerSettings.Exclusive = true;
                }
                _consumerConn = GetConnection(_consumerSettings);
                Initialize(_consumerChannel, _consumerConn, _consumerSettings);
                _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;
            }
        }

        private void Initialize(IModel channel, IConnection conn, QueueSettings settings)
        {

            // create our channels
            channel = conn.CreateModel();

            // args
            Dictionary<String, Object> args = new Dictionary<string, object>();

            if (settings.DeadLetters)
            {
                //dead letter Exchange
                string deadLetterEx = $"{settings.Queue}.dead-letter-ex";
                DeclareExchange(channel, settings, deadLetterEx);

                // dead letter queue
                string deadLetterQ = $".dead-letter-q";
                DeclareQueue(channel, settings, deadLetterQ, null);
                Bind(channel, deadLetterQ, deadLetterEx, settings.RoutingKey);

                args.Add("x-dead-letter-exchange", deadLetterEx);
                args.Add("x-dead-letter-routing-key", settings.RoutingKey);

            }
            // create ex
            DeclareExchange(channel, settings, settings.Exchange);

            // create Queue


            //if (_settings.RetryDelay > 0)
            //{
            //    // c.ExchangeDeclare(delayedExchange, "x-delayed-message", true, true, CreateProperty("x-delayed-type", "direct")


            //    string delayedExchange = $"{_settings.Queue}.delayed";
            //    args.Add("x-dead-letter-exchange", delayedExchange ?? "");
            //}
            if (settings.QoS > 0)
                channel.BasicQos(0, settings.QoS, false);

          
            DeclareQueue(channel, settings, settings.Queue, args, settings.Exclusive);
            // bind
            Bind(channel, settings.Queue, settings.Exchange, settings.RoutingKey);
        }

        public RbmqQueue(ILoggerFactory loggerFactory, RbmqConfigurationElement publisherSettings, RbmqConfigurationElement consumerSettings)
        {
            _logger = loggerFactory.CreateLogger<RbmqQueue>();


            this._publisherSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            this._consumerSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };


            _publisherSettings = new QueueSettings(publisherSettings);
            if (string.IsNullOrEmpty(_publisherSettings.Queue))
            {
                _publisherSettings.GenerateNewQueueName(true);
                _publisherSettings.GeneratedQueueName = true;
                _publisherSettings.Exclusive = true;
            }
            _publishConn = GetConnection(_publisherSettings);
            Initialize(_publishChannel, _publishConn, _publisherSettings);
            _publishConn.ConnectionShutdown += PublisherConnectionShutdown;

            _consumerSettings = new QueueSettings(consumerSettings);
            if (string.IsNullOrEmpty(_consumerSettings.Queue))
            {
                _consumerSettings.GenerateNewQueueName(false);
                _consumerSettings.GeneratedQueueName = true;
                _consumerSettings.Exclusive = true;
            }
            _consumerConn = GetConnection(_consumerSettings);
            Initialize(_consumerChannel, _consumerConn, _consumerSettings);
            _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;
            
        }

        #endregion



        #region SHUTDOWN
        private void ConsumerConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator == ShutdownInitiator.Application) return;

            _logger.LogInformation("OnShutDown: Consumer Connection broke!");

            CleanupConnection((IConnection)sender);
            
            var mres = new ManualResetEventSlim(false); // state is initially false
            
            while (!mres.Wait(3000)) // loop until state is true, checking every 3s
            {
                try
                {
                    _consumerChannel = null;
                    if(_consumerSettings.GeneratedQueueName)
                        _consumerSettings.GenerateNewQueueName(false);
                    _consumerConn = GetConnection(_consumerSettings);
                    if (_consumerConn == null) throw new Exception("Consumer connection is null");
                    Initialize(_consumerChannel, _consumerConn, _consumerSettings);
                    Subscribe();
                    _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;
                    _logger.LogInformation("Consumer Reconnected!");
                    mres.Set(); // state set to true - breaks out of loop
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consumer reconnect failed!, Error: {0}", ex.Message);
                }
            }
        }

        private void CleanupConnection(IConnection conn)
        {
            if (conn != null && conn.IsOpen)
            {
                conn.Close();
            }
        }

        private void PublisherConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator == ShutdownInitiator.Application) return;

            _logger.LogInformation("Publisher connection broke!");

            var mres = new ManualResetEventSlim(false); // state is initially false

            while (!mres.Wait(3000)) // loop until state is true, checking every 3s
            {
                try
                {
                    _publishChannel = null;
                    if(_publisherSettings.GeneratedQueueName)
                        _publisherSettings.GenerateNewQueueName(true);
                    _publishConn = GetConnection(_publisherSettings);
                    if (_publishConn == null) throw new Exception("Publisher connection is null");
                    Initialize(_publishChannel, _publishConn, _publisherSettings);
                    _publishConn.ConnectionShutdown += PublisherConnectionShutdown;

                    _logger.LogInformation("Publisher reconnected!");
                    mres.Set(); // state set to true - breaks out of loop
                }
                catch (Exception ex)
                {
                    _logger.LogError("Publisher reconnect failed!, Error: {0}", ex.Message);
                }
            }
        }
        #endregion
        /// <summary>
        /// Consumer channel. Channel where used for consumer subscription and get operations.
        /// </summary>
        public IModel ConsumerChannel
        {
            get
            {
                if (_consumerChannel == null)
                    _consumerChannel = _consumerConn.CreateModel();
                return _consumerChannel;
            }
        }

        /// <summary>
        /// Channel (IModel) used for message publishing operations
        /// </summary>
        public IModel PublishChannel
        {
            get
            {
                if (_publishChannel == null || _publishChannel.IsClosed)
                    _publishChannel = _publishConn.CreateModel();
                return _publishChannel;
            }
        }


        

        public void Enqueue(QMessage msg)
        {
            try
            {
                IBasicProperties basicProperties = PublishChannel.CreateBasicProperties();

                if (msg.Properties != null)
                {
                    basicProperties.Persistent = msg.Properties.Persistent;
                    if (!string.IsNullOrEmpty(msg.Properties.AppId))
                        basicProperties.AppId = msg.Properties.AppId;
                    if (!string.IsNullOrEmpty(msg.Properties.ClusterId))
                        basicProperties.ClusterId = msg.Properties.ClusterId;
                    if (!string.IsNullOrEmpty(msg.Properties.ContentEncoding))
                        basicProperties.ContentEncoding = msg.Properties.ContentEncoding;
                    if (!string.IsNullOrEmpty(msg.Properties.ContentType))
                        basicProperties.ContentType = msg.Properties.ContentType;
                    if (!string.IsNullOrEmpty(msg.Properties.CorrelationId))
                        basicProperties.CorrelationId = msg.Properties.CorrelationId;
                    if (msg.Properties.DeliveryMode != 0)
                        basicProperties.DeliveryMode = msg.Properties.DeliveryMode;
                    if (!string.IsNullOrEmpty(msg.Properties.Expiration))
                        basicProperties.Expiration = msg.Properties.Expiration;
                    if (!string.IsNullOrEmpty(msg.Properties.MessageId))
                        basicProperties.MessageId = msg.Properties.MessageId;
                    if (msg.Properties.Priority != 0)
                        basicProperties.Priority = msg.Properties.Priority;
                    if (!string.IsNullOrEmpty(msg.Properties.ReplyTo))
                        basicProperties.ReplyTo = msg.Properties.ReplyTo;
                    if (!string.IsNullOrEmpty(msg.Properties.Type))
                        basicProperties.Type = msg.Properties.Type;
                    if (!string.IsNullOrEmpty(msg.Properties.UserId))
                        basicProperties.UserId = msg.Properties.UserId;
                }

                // dead letters support
                //if (_settings.RetryDelay > 0)
                //{
                //    basicProperties.Headers.Add("x-delay", _settings.RetryDelay);
                //}

                lock (_lockPublisher)
                {
                    var jsonified = JsonConvert.SerializeObject(msg, Formatting.None, _publisherSerializationSettings);
                    var messageBuffer = Encoding.UTF8.GetBytes(jsonified);

                    PublishChannel.BasicPublish(_publisherSettings.Exchange, _publisherSettings.RoutingKey, basicProperties, messageBuffer);

                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Enqueue Error" + ex.Message + "Inner Exception:" + ex.InnerException);
                throw;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IConnection GetConnection(QueueSettings config)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    UserName = config.Username,
                    Password = config.Password,
                    VirtualHost = "/",
                    Protocol = Protocols.AMQP_0_9_1,
                    HostName = config.Host,
                    Port = config.Port != 0 ? config.Port : AmqpTcpEndpoint.UseDefaultPort
                };

                factory.AutomaticRecoveryEnabled = false; // automaticRecoveryEnabled;

                // VHost
                if (!string.IsNullOrEmpty(config.Vhost))
                    factory.VirtualHost = config.Vhost;

                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetConnection Error" + ex.Message + "Inner Exception:" + ex.InnerException);
                _logger.LogCritical("GetConnection Error" + ex.Message + "Inner Exception:" + ex.InnerException);
                Thread.Sleep(1000);
                this.GetConnection(config);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QMessage Dequeue<T>()
        {
            return Dequeue<T>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QMessage Dequeue<T>(bool ack = false)
        {
            QMessage msg = null;
            BasicGetResult result;
            try
            {
                //IBasicProperties basicProperties = ConsumerChannel.CreateBasicProperties();
                //basicProperties.Persistent = true;
                lock (_lockSubscriber)
                {
                    bool noAck = false;
                    // get message from queue
                    result = ConsumerChannel.BasicGet(_consumerSettings.Queue, noAck);
                    if (result == null)
                    {
                        // No message available at this time.
                    }
                    else
                    {
                        IBasicProperties props = result.BasicProperties;

                        // get body
                        byte[] body = result.Body;

                        var json = Encoding.UTF8.GetString(body);

                        json = json.Replace("\"$type\":\"Infrastructure.Queueing.Model.QMessage, Infrastructure.Queueing\",", "");
                        json = json.Replace("\"$type\":\"Infra.Quotes.Brige.Models.BridgeQuote, Infra.Quotes.Bridge\",", "");

                        msg = JsonConvert.DeserializeObject<QMessage>(json, _consumerSerializationSettings);
                        msg.DeliveryTag = result.DeliveryTag;
                        msg.Properties = new QProperties()
                        {
                            AppId = props.AppId,
                            ClusterId = props.ClusterId,
                            ContentEncoding = props.ContentEncoding,
                            ContentType = props.ContentType,
                            CorrelationId = props.CorrelationId,
                            DeliveryMode = props.DeliveryMode,
                            Expiration = props.Expiration,
                            MessageId = props.MessageId,
                            Priority = props.Priority,
                            ReplyTo = props.ReplyTo,
                            Type = props.Type,
                            UserId = props.UserId
                        };

                        if (ack)
                        {
                            ConsumerChannel.BasicAck(result.DeliveryTag, false);
                        }

                    }
                }
            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogCritical($"Dequeue Error {ex.Message},Inner Exception:{ex.InnerException}, Stack: {ex.StackTrace}");
                throw;
            }

            return msg;
        }


        public void Subscribe()
        {

            _logger.LogInformation("Subscribe: Starting...");

            Connect();

            //WaitHandle.WaitAll(new[] { waitHandle });
        }

        public void Unsubscribe()
        {     
            if(_consumer != null)
                _consumer.Received -= (model, ea) => {};

            if(_consumerConn != null)
                _consumerConn.ConnectionShutdown -= (sender, e) => { };
        }

        private void Connect()
        {
            var logString = $"hostname: {_consumerSettings.Host}, username: {_consumerSettings.Username}, password: {_consumerSettings.Password}, exchangeName: {_consumerSettings.Exchange}, " +
                $"queueName: {_consumerSettings.Queue}, isDurable: {_consumerSettings.IsDurable}, isAutodelete: {_consumerSettings.AutoDelete}, routingKey: {_consumerSettings.RoutingKey}";


            _logger.LogInformation("Connect: Connecting for {0}", logString);

            _consumer = new EventingBasicConsumer(ConsumerChannel);

            _consumer.Received += (model, ea) =>
            {
                try
                {
                    _logger.LogInformation("Consume: Calling handler for {0}", logString);

                    IBasicProperties props = ea.BasicProperties;

                    // get body
                    byte[] body = ea.Body;

                    var json = Encoding.UTF8.GetString(body);

                    var msg = JsonConvert.DeserializeObject<QMessage>(json, _consumerSerializationSettings);

                    msg.Properties = new QProperties()
                    {
                        AppId = props.AppId,
                        ClusterId = props.ClusterId,
                        ContentEncoding = props.ContentEncoding,
                        ContentType = props.ContentType,
                        CorrelationId = props.CorrelationId,
                        DeliveryMode = props.DeliveryMode,
                        Expiration = props.Expiration,
                        MessageId = props.MessageId,
                        Priority = props.Priority,
                        ReplyTo = props.ReplyTo,
                        Type = props.Type,
                        UserId = props.UserId
                    };

                    if (MessageRecieved != null)
                        MessageRecieved(model, msg);

                    //callBack(model, msg); // return byte array
                    ConsumerChannel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Consume: Acknowledged for {0}", logString);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consume: Error {0}, Stack: {1}", ex.Message, ex.StackTrace);
                }

            };


            ConsumerChannel.BasicConsume(queue: _consumerSettings.Queue, autoAck: false,
                consumer: _consumer);

            _logger.LogInformation("Consume: Connected for {0}", logString);
        }
        
        private void DeclareExchange(IModel channel, QueueSettings settings, string exchnageName)
        {
            channel.ExchangeDeclare(exchnageName, settings.ExchangeType, settings.IsDurable, false, null);
        }

        private void DeclareQueue(IModel channel, QueueSettings settings, string queueName, IDictionary<string, object> queueArgs = null, bool exclusive = false)
        {
            if (settings.Exclusive)
                settings.AutoDelete = true;

            channel.QueueDeclare(
                queue: queueName,
                durable: settings.IsDurable,
                exclusive: exclusive,
                autoDelete: settings.AutoDelete,
                arguments: queueArgs);
        }

        private void Bind(IModel channel, string queue, string ex, string routingKey)
        {
            channel.QueueBind(queue, ex, routingKey);
        }

        public void Cleanup()
        {
            try
            {
                if (_consumerChannel != null && _consumerChannel.IsOpen)
                {
                    _consumerChannel.Close();
                    _consumerChannel = null;
                }

                if (_consumerConn != null && _consumerConn.IsOpen)
                {
                    _consumerConn.Close();
                }

                if (_publishChannel != null && _publishChannel.IsOpen)
                {
                    _publishChannel.Close();
                    _publishChannel = null;
                }

                if (_publishConn != null && _publishConn.IsOpen)
                {
                    _publishConn.Close();
                }

                _logger.LogInformation("Cleanup: Done");
            }
            catch (IOException ioe)
            {
                _logger.LogError(ioe.Message);
                // Close() may throw an IOException if connection
                // dies - but that's ok (handled by reconnect)
            }
        }


        public void Clear()
        {
            try
            {
                lock (_queueLock)
                {
                    if(_consumerChannel != null && _consumerChannel.IsOpen)
                        _consumerChannel.QueuePurge(_consumerSettings.Queue);

                    if (_publishChannel != null && _publishChannel.IsOpen)
                        _publishChannel.QueuePurge(_publisherSettings.Queue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Clear failed, error: {ex.Message}");
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Cleanup();

                if(_publishChannel != null)
                    _publishChannel.Dispose();
                if(_consumerChannel != null)
                    _consumerChannel.Dispose();

                if (_consumerConn != null)
                    _consumerConn.Dispose();
                if (_publishConn != null)
                    _publishConn.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}

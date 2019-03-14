using GetInfra.Standard.Queue.Model;
using Newtonsoft.Json;
using System;

namespace GetInfra.Standard.Queue
{
    [Obsolete("use publisher or subsriber")]
    public interface IQueue
    {
        void Enqueue(QMessage msg);
        QMessage Dequeue<T>();
        void Subscribe();
        void Clear();

        event Action<object, QMessage> MessageRecieved;

        void Unsubscribe();

        void Cleanup();

        JsonSerializerSettings ConsumerSerializationSettings { get; set; }
        JsonSerializerSettings PublisherSerializationSettings { get; set; }

        QueueSettings PublisherSettings { get; }
        QueueSettings ConsumerSettings { get; }
    }
}

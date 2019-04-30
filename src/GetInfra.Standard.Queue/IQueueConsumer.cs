using GetInfra.Standard.Queue.Model;
using System;
using System.Threading.Tasks;

namespace GetInfra.Standard.Queue
{
    public interface IQueueConsumer
    {
        event Action<object, QMessage> MessageRecieved;

        void Subscribe();

        void Unsubscribe();
    }
}

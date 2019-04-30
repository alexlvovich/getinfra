using GetInfra.Standard.Queue.Model;
using System.Threading.Tasks;

namespace GetInfra.Standard.Queue
{
    public interface IQueuePublisher
    {
        Task Enqueue(QMessage msg);
    }
}

using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
public static class ServiceBusTopicTrigger1
{
    [FunctionName("jobs-transfer")]
    [ExponentialBackoffRetry(-1, "00:00:05", "00:05:00")]
    public static Task JobsTransfer(
        [ServiceBusTrigger("jobs", "replication", Connection = "jobs-left-connection")] Message[] input,
        [ServiceBus("jobs", Connection = "jobs-right-connection")] IAsyncCollector<Message> output,
            ILogger log)
    {
        return ServiceBusReplicationTasks.ForwardToServiceBus(input, output, log);
    }
}
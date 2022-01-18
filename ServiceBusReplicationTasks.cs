using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public class ServiceBusReplicationTasks
{
    public static Task ForwardToServiceBus(Message[] input, IAsyncCollector<Message> output, ILogger log)
    {
        return ConditionalForwardToServiceBus(input, output, log);
    }

    public static async Task ConditionalForwardToServiceBus(Message[] input, IAsyncCollector<Message> output, ILogger log, Func<Message, Message> factory = null)
    {
        foreach (Message message in input)
        {
            var forwardedMessage = factory != null ? factory(message) : message.Clone();

            if (forwardedMessage == null)
            {
                continue;
            }
            forwardedMessage.UserProperties[Constants.ReplEnqueuedTimePropertyName] =
                (message.UserProperties.ContainsKey(Constants.ReplEnqueuedTimePropertyName)
                    ? message.UserProperties[Constants.ReplEnqueuedTimePropertyName] + ";"
                    : string.Empty) +
                message.SystemProperties.EnqueuedTimeUtc.ToString("u");
            forwardedMessage.UserProperties[Constants.ReplOffsetPropertyName] =
                (message.UserProperties.ContainsKey(Constants.ReplOffsetPropertyName)
                    ? message.UserProperties[Constants.ReplOffsetPropertyName] + ";"
                    : string.Empty) +
                message.SystemProperties.EnqueuedSequenceNumber.ToString();
            forwardedMessage.UserProperties[Constants.ReplSequencePropertyName] =
                (message.UserProperties.ContainsKey(Constants.ReplSequencePropertyName)
                    ? message.UserProperties[Constants.ReplSequencePropertyName] + ";"
                    : string.Empty) +
                message.SystemProperties.EnqueuedSequenceNumber.ToString();
            await output.AddAsync(forwardedMessage);
        }
    }
}

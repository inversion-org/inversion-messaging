using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.SQS.Model;

using Inversion.Data;
using Inversion.Messaging.Model;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public class AmazonSQSTransport : AmazonSQSStore, ITransport
    {
        public AmazonSQSTransport(string serviceUrl, string region, string accessKey, string accessSecret) : base(serviceUrl, region, accessKey, accessSecret) {}

        public void Push(IEvent ev)
        {
            SendMessageResponse response = this.Client.SendMessage(new SendMessageRequest
            {
                MessageBody = ev.ToJson(),
                QueueUrl = this.ServiceUrl
            });
        }

        public IEvent Pop()
        {
            ReceiveMessageResponse response = this.Client.ReceiveMessage(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = 1,
                QueueUrl = this.ServiceUrl
            });

            if (response.Messages.Any())
            {
                Message message = response.Messages.First();

                if (message.Body.Length > 0)
                {
                    // remove this message from the queue
                    this.Client.DeleteMessage(new DeleteMessageRequest
                    {
                        ReceiptHandle = message.ReceiptHandle,
                        QueueUrl = this.ServiceUrl
                    });

                    return this.ConvertDocumentToEvent(message.Body);
                }
            }

            return null;
        }

        public IEvent Peek()
        {
            ReceiveMessageResponse response = this.Client.ReceiveMessage(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = 1,
                QueueUrl = this.ServiceUrl
            });

            if (response.Messages.Any())
            {
                Message message = response.Messages.First();

                if (message.Body.Length > 0)
                {
                    return this.ConvertDocumentToEvent(message.Body);
                }
            }

            return null;
        }

        public long Count()
        {
            GetQueueAttributesResponse response = this.Client.GetQueueAttributes(new GetQueueAttributesRequest
            {
                AttributeNames = new List<string> { "ApproximateNumberOfMessages" },
                QueueUrl = this.ServiceUrl
            });

            return Convert.ToInt64(response.ApproximateNumberOfMessages);
        }

        protected IEvent ConvertDocumentToEvent(string source)
        {
            return MessagingEvent.FromJson(null, source);
        }
    }
}
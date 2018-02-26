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

        public void EnsureCreated()
        {
            this.AssertIsStarted();

            CreateQueueResponse createQueueResponse;

            string queueName = "unset";

            try
            {
                ListQueuesResponse listQueuesResponse = this.Client.ListQueuesAsync(this.ServiceUrl).Result;

                if (listQueuesResponse.QueueUrls.Contains(this.ServiceUrl))
                {
                    // found our queue
                    return;
                }

                queueName = this.ServiceUrl.Substring(this.ServiceUrl.LastIndexOf('/') + 1);

                createQueueResponse = this.Client.CreateQueueAsync(queueName).Result;

                if (createQueueResponse.QueueUrl == this.ServiceUrl)
                {
                    // our queue has been created
                    return;
                }
            }
            catch (Exception)
            {
                throw new Exception(String.Format("Problem while creating queue (url: {0} name:{1})",
                    this.ServiceUrl, queueName));
            }

            throw new Exception(String.Format("Created queue didn't match service url.\r\nExpected: {0}\r\nReceived: {1}\r\n", this.ServiceUrl, createQueueResponse.QueueUrl));
        }

        public void Push(IEvent ev)
        {
            this.AssertIsStarted();

            SendMessageResponse response = this.Client.SendMessageAsync(new SendMessageRequest
            {
                MessageBody = ev.ToJson(),
                QueueUrl = this.ServiceUrl
            }).Result;
        }

        public IEvent Pop()
        {
            this.AssertIsStarted();

            ReceiveMessageResponse response = this.Client.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = 1,
                QueueUrl = this.ServiceUrl
            }).Result;

            if (response.Messages.Any())
            {
                Message message = response.Messages.First();

                if (message.Body.Length > 0)
                {
                    // remove this message from the queue
                    DeleteMessageResponse deleteMessageResponse = this.Client.DeleteMessageAsync(new DeleteMessageRequest
                    {
                        ReceiptHandle = message.ReceiptHandle,
                        QueueUrl = this.ServiceUrl
                    }).Result;

                    return this.ConvertDocumentToEvent(message.Body);
                }
            }

            return null;
        }

        public IEvent Peek()
        {
            this.AssertIsStarted();

            ReceiveMessageResponse response = this.Client.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = 1,
                QueueUrl = this.ServiceUrl
            }).Result;

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
            this.AssertIsStarted();

            GetQueueAttributesResponse response = this.Client.GetQueueAttributesAsync(new GetQueueAttributesRequest
            {
                AttributeNames = new List<string> { "ApproximateNumberOfMessages" },
                QueueUrl = this.ServiceUrl
            }).Result;

            return Convert.ToInt64(response.ApproximateNumberOfMessages);
        }

        protected IEvent ConvertDocumentToEvent(string source)
        {
            return MessagingEvent.FromJson(null, source);
        }
    }
}
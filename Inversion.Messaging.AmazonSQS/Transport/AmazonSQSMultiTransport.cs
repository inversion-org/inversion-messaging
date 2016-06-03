using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.SQS.Model;

using Inversion.Data;
using Inversion.Messaging.Model;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public class AmazonSQSMultiTransport : AmazonSQSStore, ITransport
    {
        private readonly string _serviceUrlRegex;

        private readonly List<string> _serviceUrls = new List<string>();

        public AmazonSQSMultiTransport(string baseServiceUrl, string serviceUrlRegex, string region, string accessKey,
            string accessSecret) : base(baseServiceUrl, region, accessKey, accessSecret)
        {
            _serviceUrlRegex = serviceUrlRegex;
        }

        public override void Start()
        {
            base.Start();

            ListQueuesResponse listQueuesResponse = this.Client.ListQueues(new ListQueuesRequest());

            Regex regex = new Regex(_serviceUrlRegex);
            
            _serviceUrls.AddRange(listQueuesResponse.QueueUrls.Where(url => regex.IsMatch(url)));
        }

        public void Push(IEvent ev)
        {
            this.AssertIsStarted();

            this.Client.SendMessage(new SendMessageRequest
            {
                MessageBody = ev.ToJson(),
                QueueUrl = this.ServiceUrl
            });
        }

        public IEvent Pop()
        {
            this.AssertIsStarted();

            return this.Pop(withDelete: true);
        }

        protected IEvent Pop(bool withDelete)
        {
            foreach (string serviceUrl in _serviceUrls)
            {
                ReceiveMessageResponse response = this.Client.ReceiveMessage(new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = 1,
                    QueueUrl = serviceUrl
                });

                if (response.Messages.Any())
                {
                    Message message = response.Messages.First();

                    if (message.Body.Length > 0)
                    {
                        if (withDelete)
                        {
                            // remove this message from the queue
                            this.Client.DeleteMessage(new DeleteMessageRequest
                            {
                                ReceiptHandle = message.ReceiptHandle,
                                QueueUrl = this.ServiceUrl
                            });
                        }

                        return this.ConvertDocumentToEvent(message.Body, serviceUrl);
                    }
                }
            }

            return null;
        }

        public IEvent Peek()
        {
            this.AssertIsStarted();

            return this.Pop(withDelete: false);
        }

        public long Count()
        {
            this.AssertIsStarted();

            long count = 0;

            foreach (string serviceUrl in _serviceUrls)
            {
                GetQueueAttributesResponse response = this.Client.GetQueueAttributes(new GetQueueAttributesRequest
                {
                    AttributeNames = new List<string> { "ApproximateNumberOfMessages" },
                    QueueUrl = serviceUrl
                });

                count += Convert.ToInt64(response.ApproximateNumberOfMessages);
            }

            return count;
        }

        protected IEvent ConvertDocumentToEvent(string source, string serviceUrl)
        {
            IEvent ev = MessagingEvent.FromJson(null, source);
            ev.Params["_service-url"] = serviceUrl;

            return ev;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Amazon.SQS.Model;

using Inversion.Data;
using Inversion.Messaging.Model;
using Inversion.Process;
using log4net;

namespace Inversion.Messaging.Transport
{
    public class AmazonSQSMultiTransport : AmazonSQSStore, ITransport
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Random _random = new Random();

        private readonly string _serviceUrlRegex;

        private readonly List<string> _serviceUrls = new List<string>();

        private readonly bool _popFromRandomQueue;

        public AmazonSQSMultiTransport(string baseServiceUrl, string serviceUrlRegex, string region, string accessKey,
            string accessSecret, bool popFromRandomQueue = false) : base(baseServiceUrl, region, accessKey, accessSecret)
        {
            _serviceUrlRegex = serviceUrlRegex;
            _popFromRandomQueue = popFromRandomQueue;
        }

        public override void Start()
        {
            base.Start();

            ListQueuesResponse listQueuesResponse = this.Client.ListQueues(new ListQueuesRequest());

            Regex regex = new Regex(_serviceUrlRegex);
            
            _serviceUrls.AddRange(listQueuesResponse.QueueUrls.Where(url => regex.IsMatch(url)));

            //_log.InfoFormat("AmazonSQSMultiTransport.Start: service urls: {0}", String.Join("\t", _serviceUrls));
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
            if (_popFromRandomQueue)
            {
                return this.PopFromRandomQueue(withDelete);
            }

            return this.PopCyclic(withDelete);
        }

        protected IEvent PopFromRandomQueue(bool withDelete)
        {
            string serviceUrl = _serviceUrls[_random.Next(_serviceUrls.Count)];

            ReceiveMessageResponse response = this.Client.ReceiveMessage(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = 1,
                QueueUrl = serviceUrl
            });

            if (response.Messages.Any())
            {
                //_log.InfoFormat("pop from: {0}", serviceUrl);

                Message message = response.Messages.First();

                if (message.Body.Length > 0)
                {
                    if (withDelete)
                    {
                        // remove this message from the queue
                        this.Client.DeleteMessage(new DeleteMessageRequest
                        {
                            ReceiptHandle = message.ReceiptHandle,
                            QueueUrl = serviceUrl
                        });
                    }

                    return this.ConvertDocumentToEvent(message.Body, serviceUrl);
                }

                return null;
            }
            return PopCyclic(withDelete);
        }

        protected IEvent PopCyclic(bool withDelete)
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
                                QueueUrl = serviceUrl
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
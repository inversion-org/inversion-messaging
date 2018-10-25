using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
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

        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private static readonly Random _random = new Random();
        private DateTime _lastCheck = DateTime.MinValue;
        private readonly TimeSpan _recheck = new TimeSpan(0, 1, 0);

        private readonly string _serviceUrlRegex;
        private readonly List<string> _auxiliaryServiceUrls;

        private List<string> _serviceUrls = new List<string>();

        private readonly bool _popFromRandomQueue;

        public AmazonSQSMultiTransport(string baseServiceUrl, string serviceUrlRegex, string region, string accessKey="",
            string accessSecret="", List<string> auxiliaryServiceUrls = null, bool popFromRandomQueue = false) : base(baseServiceUrl, region, accessKey, accessSecret)
        {
            _serviceUrlRegex = serviceUrlRegex;
            _popFromRandomQueue = popFromRandomQueue;
            _auxiliaryServiceUrls = auxiliaryServiceUrls;
        }

        public override void Start()
        {
            base.Start();

            EnsureServiceUrlsUpToDate();

            //_log.InfoFormat("AmazonSQSMultiTransport.Start: service urls: {0}", String.Join("\t", _serviceUrls));
        }

        protected void EnsureServiceUrlsUpToDate()
        {
            try
            {
                _lock.EnterUpgradeableReadLock();

                if (_lastCheck.Add(_recheck).Ticks < DateTime.Now.Ticks)
                {
                    _lock.EnterWriteLock();

                    ListQueuesResponse listQueuesResponse = this.Client.ListQueuesAsync(new ListQueuesRequest()).Result;

                    Regex regex = new Regex(_serviceUrlRegex);

                    _serviceUrls = new List<string>(listQueuesResponse.QueueUrls.Where(url =>
                        regex.IsMatch(url) ||
                        (_auxiliaryServiceUrls != null && _auxiliaryServiceUrls.Contains(url))));

                    _lastCheck = DateTime.Now;

                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
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
            this.EnsureServiceUrlsUpToDate();

            string serviceUrl = _serviceUrls[_random.Next(_serviceUrls.Count)];

            ReceiveMessageResponse response = this.Client.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = 1,
                QueueUrl = serviceUrl
            }).Result;

            if (response.Messages.Any())
            {
                //_log.InfoFormat("pop from: {0}", serviceUrl);

                Message message = response.Messages.First();

                if (message.Body.Length > 0)
                {
                    if (withDelete)
                    {
                        // remove this message from the queue
                        DeleteMessageResponse deleteMessageResponse = this.Client.DeleteMessageAsync(new DeleteMessageRequest
                        {
                            ReceiptHandle = message.ReceiptHandle,
                            QueueUrl = serviceUrl
                        }).Result;
                    }

                    return this.ConvertDocumentToEvent(message.Body, serviceUrl);
                }

                return null;
            }
            return PopCyclic(withDelete);
        }

        protected IEvent PopCyclic(bool withDelete)
        {
            this.EnsureServiceUrlsUpToDate();

            foreach (string serviceUrl in _serviceUrls)
            {
                ReceiveMessageResponse response = this.Client.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = 1,
                    QueueUrl = serviceUrl
                }).Result;

                if (response.Messages.Any())
                {
                    Message message = response.Messages.First();

                    if (message.Body.Length > 0)
                    {
                        if (withDelete)
                        {
                            // remove this message from the queue
                            DeleteMessageResponse deleteMessageResponse = this.Client.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                ReceiptHandle = message.ReceiptHandle,
                                QueueUrl = serviceUrl
                            }).Result;
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

            this.EnsureServiceUrlsUpToDate();

            foreach (string serviceUrl in _serviceUrls)
            {
                GetQueueAttributesResponse response = this.Client.GetQueueAttributesAsync(new GetQueueAttributesRequest
                {
                    AttributeNames = new List<string> { "ApproximateNumberOfMessages" },
                    QueueUrl = serviceUrl
                }).Result;

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
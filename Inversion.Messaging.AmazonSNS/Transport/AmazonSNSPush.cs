using System;

using Amazon.SimpleNotificationService.Model;

using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public class AmazonSNSPush : AmazonSNSStore, IPush
    {
        public AmazonSNSPush(string topicArn, string region, string accessKey="", string accessSecret="", string serviceUrl="") : base(topicArn, region, accessKey, accessSecret, serviceUrl) {}

        public void Push(IEvent ev)
        {
            this.AssertIsStarted();

            PublishResponse response = this.Client.PublishAsync(new PublishRequest
            {
                Message = ev.ToJson(),
                TopicArn = this.TopicArn
            }).Result;
        }
    }
}
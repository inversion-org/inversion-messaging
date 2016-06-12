using System;

using Amazon.SimpleNotificationService.Model;

using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public class AmazonSNSPush : AmazonSNSStore, IPush
    {
        public AmazonSNSPush(string serviceUrl, string region, string accessKey, string accessSecret) : base(serviceUrl, region, accessKey, accessSecret) {}

        public void Push(IEvent ev)
        {
            this.AssertIsStarted();

            PublishResponse response = this.Client.Publish(new PublishRequest
            {
                Message = ev.ToJson(),
                TopicArn = this.TopicArn
            });
        }
    }
}
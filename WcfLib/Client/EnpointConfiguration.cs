namespace WcfLib.Client
{
    using System.ServiceModel;

    public class EnpointConfiguration<TService>
    {
        public EnpointConfiguration(ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy)
        {
            ChannelFactory = channelFactory;
            RetryPolicy = retryPolicy;
        }

        public ChannelFactory<TService> ChannelFactory { get; set; }
        public RetryPolicy RetryPolicy { get; set; }
    }
}
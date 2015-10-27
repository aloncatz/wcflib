namespace WcfLib.Client
{
    using System;
    using System.ServiceModel;

    public interface IWcfClientFactory
    {
        void Register<TService>(ChannelFactory<TService> channelFactory);
        void Register<TService>(ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy);
        void Register<TService>(string name, ChannelFactory<TService> channelFactory);
        void Register<TService>(string name, ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy);
        void Register<TService>(Func<EnpointConfiguration<TService>> endpointConfigurationFactory);
        void Register<TService>(string name, Func<EnpointConfiguration<TService>> endpointConfigurationFactory);

        WcfClient<TService> CreateClient<TService>();
        WcfClient<TService> CreateClient<TService>(string name);
    }
}
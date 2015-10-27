namespace WcfLib.Client
{
    using System.ServiceModel;

    public interface IWcfClientFactory
    {
        void Register<TService>(ChannelFactory<TService> channelFactory);
        void Register<TService>(ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy);
        void Register<TService>(string name, ChannelFactory<TService> channelFactory);
        void Register<TService>(string name, ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy);
        WcfClient<TService> CreateClient<TService>();
        WcfClient<TService> CreateClient<TService>(string name);
    }
}
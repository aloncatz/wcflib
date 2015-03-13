using System;
using System.Collections.Concurrent;
using System.ServiceModel;

namespace WcfLib.Client
{
    public class WcfClientFactory
    {
        private ConcurrentDictionary<string, EndpointRegistration> _channelFactories = new ConcurrentDictionary<string, EndpointRegistration>();

        public void Register<TService>(ChannelFactory<TService> channelFactory)
        {
            Register(null, channelFactory);
        }

        public void Register<TService>(ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy)
        {
            Register(null, channelFactory, retryPolicy);
        }

        public void Register<TService>(string name, ChannelFactory<TService> channelFactory)
        {
            Register(name, channelFactory, new NoRetryPolicy());
        }

        public void Register<TService>(string name, ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy)
        {
            if (channelFactory == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }

            string key = GetCacheKey<TService>(name);
            var endpointRegisration = new EndpointRegistration(name, new WcfChannelPool<TService>(channelFactory), retryPolicy);
            _channelFactories.AddOrUpdate(key, endpointRegisration, (s, er) => { throw new Exception("This ChannelFactory is already registered"); });
        }

        public WcfClient<TService> CreateClient<TService>()
        {
            return CreateClient<TService>(null);
        }

        public WcfClient<TService> CreateClient<TService>(string name)
        {
            string cacheKey = GetCacheKey<TService>(name);
            if (!_channelFactories.ContainsKey(cacheKey))
            {
                throw new ArgumentException("ChannelFactory for this service isn't registered. Use Register before calling CreateClient");
            }
            var reg = _channelFactories[cacheKey];
            return new WcfClient<TService>(reg.ChannelPool, reg.RetryPolicy);
        }

        private string GetCacheKey<TService>(string name)
        {
            name = name ?? "default";
            return name + "#" + typeof (TService).FullName;
        }
    }

    public class EndpointRegistration
    {
        public EndpointRegistration(string name, IWcfChannelPool channelPool, RetryPolicy retryPolicy)
        {
            Name = name;
            ChannelPool = channelPool;
            RetryPolicy = retryPolicy;
        }

        public IWcfChannelPool ChannelPool { get; set; }
        public string Name { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }
    }
}
using System;
using System.Collections.Concurrent;
using System.ServiceModel;

namespace WcfLib.Client
{
    public class WcfClientFactory : IWcfClientFactory
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

            var endpointRegisration = CreateEndpointRegistration(name, channelFactory, retryPolicy);
            _channelFactories.AddOrUpdate(endpointRegisration.Key, endpointRegisration, (s, er) => { throw new Exception("This ChannelFactory is already registered"); });
        }

        public void Register<TService>(Func<EnpointConfiguration<TService>> endpointConfigurationFactory)
        {
            Register(null, endpointConfigurationFactory);    
        }

        public void Register<TService>(string name, Func<EnpointConfiguration<TService>> endpointConfigurationFactory)
        {
            if (endpointConfigurationFactory == null)
            {
                throw new ArgumentNullException("endpointConfigurationFactory");
            }
            
            string key = GetCacheKey<TService>(name);

            //Only call the endpointRegistrationFactory method if _channelFactories has no value for this key
            _channelFactories.GetOrAdd(key, s =>
            {
                var endpointData = endpointConfigurationFactory();
                var endpointRegisration = CreateEndpointRegistration(name, endpointData.ChannelFactory, endpointData.RetryPolicy);
                return endpointRegisration;
            });
        }

        private EndpointRegistration CreateEndpointRegistration<TService>(string name, ChannelFactory<TService> channelFactory, RetryPolicy retryPolicy)
        {
            string key = GetCacheKey<TService>(name);
            var endpointRegisration = new EndpointRegistration(key, name, new WcfChannelPool<TService>(channelFactory), retryPolicy);
            return endpointRegisration;
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

        class EndpointRegistration
        {
            public EndpointRegistration(string key, string name, IWcfChannelPool channelPool, RetryPolicy retryPolicy)
            {
                Key = key;
                Name = name;
                ChannelPool = channelPool;
                RetryPolicy = retryPolicy;
            }

            public string Key { get; private set; }
            public IWcfChannelPool ChannelPool { get; private set; }
            public string Name { get; private set; }
            public RetryPolicy RetryPolicy { get; private set; }
        }
    }
}
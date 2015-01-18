using System;
using System.Collections.Concurrent;
using System.ServiceModel;

namespace WcfLib.Client
{
    public class WcfClientFactory
    {
        private ConcurrentDictionary<string, ChannelFactory> _channelFactories = new ConcurrentDictionary<string, ChannelFactory>();

        private ConcurrentDictionary<string, object> _clients = new ConcurrentDictionary<string, object>();

        public void Register<TService>(ChannelFactory<TService> channelFactory)
        {
            Register(null, channelFactory);
        }

        public void Register<TService>(string name, ChannelFactory<TService> channelFactory)
        {
            string key = GetCacheKey<TService>(name);
            _channelFactories.AddOrUpdate(key, channelFactory, (s, factory) => { throw new Exception("This ChannelFactory is already registered"); });
        }

        public WcfClient<TService> GetClient<TService>()
        {
            return GetClient<TService>(null);
        }

        public WcfClient<TService> GetClient<TService>(string name)
        {
            string cacheKey = GetCacheKey<TService>(name);
            object client = _clients.GetOrAdd(cacheKey, x =>
            {
                if (!_channelFactories.ContainsKey(cacheKey))
                {
                    throw new ArgumentException("ChannelFactory for this service isn't registered. Use Register before calling GetClient");
                }
                var channelFactory = (ChannelFactory<TService>) _channelFactories[cacheKey];
                return new WcfClient<TService>(channelFactory);
            });

            return (WcfClient<TService>) client;
        }

        private string GetCacheKey<TService>(string name)
        {
            name = name ?? "default";
            return name + "#" + typeof (TService).FullName;
        }
    }
}
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfLib.Client
{
    public class WcfClient<TService>
    {
        private readonly WcfChannelPool<TService> _channelPool;

        public WcfClient(ChannelFactory<TService> channelFactory) : this(new WcfChannelPool<TService>(channelFactory))
        {
        }

        public WcfClient(WcfChannelPool<TService> channelPool)
        {
            _channelPool = channelPool;
        }

        /// <summary>
        ///     Returns the channel factory used by this client
        /// </summary>
        public ChannelFactory<TService> ChannelFactory { get { return _channelPool.ChannelFactory; }}

        public async Task Call(Func<TService, Task> action)
        {
            Func<TService, Task<int>> actionWrapper = async service =>
            {
                await action(service);
                return 0;
            };

            await Call(actionWrapper);
        }

        public async Task<TResult> Call<TResult>(Func<TService, Task<TResult>> action)
        {
            return await CallCore(action);
        }

        private async Task<TResult> CallCore<TResult>(Func<TService, Task<TResult>> action)
        {
            // Get a good channel from the pool and release it to the pool when we are done
            // If there was an error, abort the channel and don't return it to the pool
            IClientChannel channel = await _channelPool.GetChannel();
            try
            {
                TResult result = await action((TService) channel);
                _channelPool.ReleaseChannel(channel);
                return result;
            }
            catch (Exception)
            {
                if (channel.State == CommunicationState.Opened)
                {
                    // Depending on the way the exception was generated on the server, the channel may or may not become faulted
                    // If the channel is healthy, return it to the pool
                    _channelPool.ReleaseChannel(channel);
                }
                else
                {
                    // All other exceptions (this should mainly be  CommunicationException)
                    // The channel is assumed to be broken. We abort it and don't return it to the pool.
                    channel.Abort();
                }
                throw;
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Reeb.Wcf
{
    public class WcfClient<TService>
    {
        readonly WcfChannelPool<TService> _channelPool;

        public WcfClient(ChannelFactory<TService> channelFactory) : this (new WcfChannelPool<TService>(channelFactory))
        {
        }

        public WcfClient(WcfChannelPool<TService> channelPool)
        {
            _channelPool = channelPool;
        }

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
            IClientChannel channel = _channelPool.GetChannel();
            try
            {
                TResult result = await action((TService) channel);
                _channelPool.ReleaseChannel(channel);
                return result;
            }
            catch (FaultException)
            {
                // FaultException represents a user error in the service,
                // The channel is assumed to survive such errors, and shouldn't be recycled
                _channelPool.ReleaseChannel(channel);
                throw;
            }
            catch (Exception)
            {
                // All other exceptions (this should mainly be  CommunicationException)
                // The channel is assumed to be broken. We abort it and don't return it to the pool.
                channel.Abort();
                throw;
            }
        }
    }
}
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfLib.Client
{
    public class WcfClient<TService>
    {
        private readonly RetryPolicy _retryPolicy;
        private readonly WcfChannelPool<TService> _channelPool;

        public WcfClient(WcfChannelPool<TService> channelPool, RetryPolicy retryPolicy)
        {
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("channelPool");
            }
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }

            _channelPool = channelPool;
            _retryPolicy = retryPolicy;
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
            int maxRetryCount = Math.Max(1, _retryPolicy.MaxRetryCount);
            for (int retryIndex = 0; retryIndex < maxRetryCount; retryIndex++)
            {
                try
                {
                    return await CallCore(action);
                }
                catch (Exception)
                {
                    // If this was the last retry, give up and throw
                    if (retryIndex == maxRetryCount - 1)
                        throw;
                }

                await Task.Delay(_retryPolicy.GetDelay(retryIndex));
            }

            throw new Exception("Retries were exhausted but no result was return and no exception was thrown. This shouldn't have happened...");
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
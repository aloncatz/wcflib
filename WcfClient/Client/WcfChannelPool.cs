using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfLib.Client
{
    public class WcfChannelPool<TService>
    {
        private readonly ChannelFactory<TService> _channelFactory;
        private readonly ConcurrentQueue<IClientChannel> _pool = new ConcurrentQueue<IClientChannel>();

        public WcfChannelPool(ChannelFactory<TService> channelFactory)
        {
            _channelFactory = channelFactory;
        }

        /// <summary>
        ///     Returns the current count of channels in the pool. There could be additional channels which are
        ///     being used and are not reflected in this count
        /// </summary>
        public int PoolSize
        {
            get { return _pool.Count; }
        }

        /// <summary>
        ///     Returns the channel factory used by this pool
        /// </summary>
        public ChannelFactory<TService> ChannelFactory { get { return _channelFactory; } }

        public virtual async Task<IClientChannel> GetChannel()
        {
            IClientChannel channel = GetGoodChannelFromPool();
            if (channel == null)
            {
                channel = (IClientChannel) _channelFactory.CreateChannel();
                await Task.Factory.FromAsync(channel.BeginOpen, channel.EndOpen, null);
            }
            return channel;
        }

        /// <summary>
        /// Return the channel into the pool. Only healthy Opened channels should come back into the pool
        /// </summary>
        /// <param name="channel"></param>
        public virtual void ReleaseChannel(IClientChannel channel)
        {
            if (channel.State != CommunicationState.Opened)
            {
                throw new ArgumentException("Released channels must be healthy and in CommunicationState.Opened state", "channel");
            }
            _pool.Enqueue(channel);
        }

        /// <summary>
        /// Returns a channel with State == CommunicationState.Opened or null
        /// </summary>
        private IClientChannel GetGoodChannelFromPool()
        {
            IClientChannel goodChannel = null;
            bool done = false;
            while (!done)
            {
                IClientChannel channel;
                if (!_pool.TryDequeue(out channel))
                {
                    //If there is nothing in the queue, we are done. No channel.
                    done = true;
                }
                else if (channel.State == CommunicationState.Opened)
                {
                    //If we found a good channel, use it
                    goodChannel = channel;
                    done = true;
                }
                else
                {
                    //If the channel state is other than opened, assume it is broken and Abort it to clear
                    //system resources
                    channel.Abort();
                }
            }
            return goodChannel;
        }
    }
}
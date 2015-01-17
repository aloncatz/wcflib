using System.Collections.Concurrent;
using System.ServiceModel;

namespace Reeb.Wcf
{
    public class WcfChannelPool<TService>
    {
        private readonly ChannelFactory<TService> _channelFactory;
        private readonly ConcurrentQueue<IClientChannel> _pool = new ConcurrentQueue<IClientChannel>();

        public WcfChannelPool(ChannelFactory<TService> channelFactory)
        {
            _channelFactory = channelFactory;
        }


        public virtual IClientChannel GetChannel()
        {
            IClientChannel channel = GetGoodChannelFromPool();
            if (channel == null)
            {
                channel = (IClientChannel) _channelFactory.CreateChannel();
                channel.Open();
            }
            return channel;
        }

        public virtual void ReleaseChannel(IClientChannel channel)
        {
            _pool.Enqueue(channel);
        }

        private IClientChannel GetGoodChannelFromPool()
        {
            IClientChannel goodChannel = null;
            bool done = false;
            while (!done)
            {
                IClientChannel channel;
                if (!_pool.TryDequeue(out channel))
                {
                    done = true;
                }
                else if (channel.State == CommunicationState.Opened)
                {
                    goodChannel = channel;
                    done = true;
                }
            }
            return goodChannel;
        }
    }
}
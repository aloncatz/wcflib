using System;

namespace WcfLib.Client
{
    public abstract class RetryPolicy
    {
        public int MaxRetryCount { get; protected set; }
        public abstract TimeSpan GetDelay(int attemptNumber);
    }

    public class NoRetryPolicy : RetryPolicy
    {
        public NoRetryPolicy()
        {
            MaxRetryCount = 0;
        }

        public override TimeSpan GetDelay(int attemptNumber)
        {
            return TimeSpan.Zero;
        }
    }

    public class LinearRetryPolicy : RetryPolicy
    {
        private TimeSpan retryDelay;

        public LinearRetryPolicy(int maxRetryCount, TimeSpan retryDelay)
        {
            MaxRetryCount = maxRetryCount;

            this.retryDelay = retryDelay;
        }

        public override TimeSpan GetDelay(int attemptNumber)
        {
            return retryDelay;
        }
    }
}
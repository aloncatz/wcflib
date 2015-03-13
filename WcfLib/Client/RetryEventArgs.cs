namespace WcfLib.Client
{
    public class RetryEventArgs
    {
        public RetryEventArgs(int attemptNumber)
        {
            AttemptNumber = attemptNumber;
        }

        public int AttemptNumber { get; private set; }
    }
}
namespace WcfLib.Client
{
    using System;

    public class RetryEventArgs
    {
        public RetryEventArgs(int attemptNumber, Exception error)
        {
            Error = error;
            AttemptNumber = attemptNumber;
        }

        public Exception Error { get; private set; }
        public int AttemptNumber { get; private set; }
    }
}
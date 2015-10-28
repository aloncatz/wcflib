namespace WcfLib.Client
{
    using System;
    using System.Threading.Tasks;

    public interface IWcfClient<T>
    {
        event EventHandler<RetryEventArgs> TransientFailure;
        Task Call(Func<T, Task> action);
        Task<TResult> Call<TResult>(Func<T, Task<TResult>> action);
    }
}
using Moq;

namespace Reeb.Wcf.Test.TestFramework
{
    public static class MoqExtensions
    {
        /// <summary>
        ///     Similar to Mock's <see cref="It.IsAny{T}" /> but even more generic because it doesn't test the
        ///     object derives from <see cref="TValue" />
        /// </summary>
        public static TValue AnyObject<TValue>()
        {
            return Match.Create(
                value => true,
                () => AnyObject<TValue>());
        }
    }
}
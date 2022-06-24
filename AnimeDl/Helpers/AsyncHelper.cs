using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeDl.Helpers
{
    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(
            Func<Task<TResult>> func,
            CancellationToken cancellationToken = default)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }, cancellationToken).Unwrap().GetAwaiter().GetResult();
        }

        public static void RunSync(
            Func<Task> func,
            CancellationToken cancellationToken = default)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }, cancellationToken).Unwrap().GetAwaiter().GetResult();
        }
    }
}
using Microsoft.IdentityModel.Protocols;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace Host.Configuration.Extensions
{
    class SyncConfigurationManager : IConfigurationManager<WsFederationConfiguration>
    {
        private readonly IConfigurationManager<WsFederationConfiguration> _inner;

        public SyncConfigurationManager(IConfigurationManager<WsFederationConfiguration> inner)
        {
            _inner = inner;
        }

        public Task<WsFederationConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            var res = AsyncHelper.RunSync(() => _inner.GetConfigurationAsync(cancel));
            return Task.FromResult(res);
        }

        public void RequestRefresh()
        {
            _inner.RequestRefresh();
        }

        private static class AsyncHelper
        {
            private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
                TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

            public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            {
                var cultureUi = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.CurrentCulture;
                return _myTaskFactory.StartNew(() =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                }).Unwrap().GetAwaiter().GetResult();
            }

            public static void RunSync(Func<Task> func)
            {
                var cultureUi = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.CurrentCulture;
                _myTaskFactory.StartNew(() =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                }).Unwrap().GetAwaiter().GetResult();
            }
        }
    }
}
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.ChatHubs.Services;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class OnStartupApplicationTask : HostedServiceBase
    {
        private ILogger<CachingFrontpageTask> logger { get; set; }
        private bool Executed { get; set; }

        public OnStartupApplicationTask(IServiceScopeFactory serviceScopeFactory, ILogger<CachingFrontpageTask> logger) : base(serviceScopeFactory)
        {
            Name = "On Startup Application Task";
            Frequency = "m";
            Interval = 1;
            IsEnabled = true;
            this.logger = logger;
        }

        public override string ExecuteJob(IServiceProvider provider)
        {
            if (this.Executed == false)
            {
                this.Executed = true;
                var siteRepository = provider.GetRequiredService<ISiteRepository>();
                var moduleRepository = provider.GetRequiredService<IModuleRepository>();
                var chatHubService = provider.GetRequiredService<ChatHubService>();

                chatHubService.ArchiveActiveDbItems().GetAwaiter().GetResult();
            }

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.ChatHubs.Services;
using Oqtane.Models;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class CachingFrontpageTask : HostedServiceBase
    {
        private ILogger<CachingFrontpageTask> logger { get; set; }

        public CachingFrontpageTask(IServiceScopeFactory serviceScopeFactory, ILogger<CachingFrontpageTask> logger) : base(serviceScopeFactory)
        {
            Name = "Caching Frontpages Task";
            Frequency = "s";
            Interval = 28;
            IsEnabled = true;
            this.logger = logger;
        }

        public override string ExecuteJob(IServiceProvider provider)
        {
            string log = "";

            this.logger.LogInformation("Caching frontpage task getting services.");

            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var moduleRepository = provider.GetRequiredService<IModuleRepository>();
            var chatHubService = provider.GetRequiredService<ChatHubService>();

            this.logger.LogInformation("Caching frontpage task getting sites list.");
            List<Site> sites = siteRepository.GetSites().ToList();
            foreach (var site in sites)
            {
                log += "Processing Frontpage For Site: " + site.Name + "<br />";

                this.logger.LogInformation("Caching frontpage task getting modules.");
                var modules = moduleRepository.GetModules(site.SiteId);
                foreach (var module in modules.Where(item => item.ModuleDefinitionName == "Oqtane.ChatHubs, Oqtane.ChatHubs.Client.Oqtane"))
                {
                    this.logger.LogInformation("Caching frontpage task try caching.");
                    var items = chatHubService.GetRooms(1, Oqtane.ChatHubs.Constants.ChatHubConstants.FrontPageItems, module.ModuleId, true).GetAwaiter().GetResult();
                    log += "Caching Frontpage Succeeded For Module: " + module.ModuleId + "<br />";
                }
            }

            return log;
        }
    }
}
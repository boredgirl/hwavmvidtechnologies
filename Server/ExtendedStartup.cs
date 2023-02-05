using System;
using BlazorAlerts;
using BlazorBrowserResize;
using Hwavmvid.ColorPicker;
using BlazorDraggableList;
using Hwavmvid.FileUpload;
using BlazorModal;
using Hwavmvid.Video;
using BlazorNotifications;
using Hwavmvid.Pager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.ChatHubs.Hubs;
using Oqtane.Infrastructure;
using BlazorDynamicLayout;
using Oqtane.ChatHubs.Models;
using Hwavmvid.VideoPlayer;
using BlazorSlider;
using BlazorDevices;
using BlazorDownload;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Hwavmvid.Jsapinotifications;
using Hwavmvid.Jsapigeolocation;
using Hwavmvid.Blackjack;
using Hwavmvid.Roulette;
using Hwavmvid.Rouletteitellisense;
using Hwavmvid.Roulettesurface;
using Hwavmvid.Roulettecoins;
using Hwavmvid.Roulettebetoptions;
using Hwavmvid.Roulettebets;
using Hwavmvid.Motorsport.Racewaymaps;
using Oqtane.Security;

namespace Oqtane
{
    public class ExtendedStartup : IServerStartup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                options.JsonSerializerOptions.DefaultBufferSize = 4096;
                options.JsonSerializerOptions.MaxDepth = 41;
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddMemoryCache();
            services.TryAddHttpClientWithAuthenticationCookie();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<BlazorAlertsService, BlazorAlertsService>();
            services.AddScoped<BlazorDraggableListService, BlazorDraggableListService>();
            services.AddScoped<FileUploadService, FileUploadService>();
            services.AddScoped<ColorPickerService, ColorPickerService>();
            services.AddScoped<VideoService, VideoService>();
            services.AddScoped<VideoPlayerService, VideoPlayerService>();
            services.AddScoped<BlazorBrowserResizeService, BlazorBrowserResizeService>();
            services.AddScoped<BlazorModalService, BlazorModalService>();
            services.AddScoped<BlazorNotificationsService, BlazorNotificationsService>();
            services.AddScoped<BlazorDynamicLayoutService, BlazorDynamicLayoutService>();
            services.AddScoped<BlazorSliderService, BlazorSliderService>();
            services.AddScoped<BlazorDevicesService, BlazorDevicesService>();
            services.AddScoped<BlazorDownloadService, BlazorDownloadService>();
            services.AddScoped<PagerService<ChatHubRoom>, PagerService<ChatHubRoom>>();
            services.AddScoped<PagerService<ChatHubUser>, PagerService<ChatHubUser>>();
            services.AddScoped<PagerService<ChatHubCam>, PagerService<ChatHubCam>>();
            services.AddScoped<PagerService<ChatHubInvitation>, PagerService<ChatHubInvitation>>();
            services.AddScoped<PagerService<ChatHubIgnore>, PagerService<ChatHubIgnore>>();
            services.AddScoped<PagerService<ChatHubIgnoredBy>, PagerService<ChatHubIgnoredBy>>();
            services.AddScoped<PagerService<ChatHubModerator>, PagerService<ChatHubModerator>>();
            services.AddScoped<PagerService<ChatHubBlacklistUser>, PagerService<ChatHubBlacklistUser>>();
            services.AddScoped<PagerService<ChatHubWhitelistUser>, PagerService<ChatHubWhitelistUser>>();
            services.AddScoped<JsapinotificationService, JsapinotificationService>();
            services.AddScoped<Jsapigeolocationservice, Jsapigeolocationservice>();
            services.AddScoped<Jsapibingmapservice, Jsapibingmapservice>();
            services.AddScoped<BlackjackService, BlackjackService>();
            services.AddScoped<RouletteService, RouletteService>();
            services.AddScoped<RoulettesurfaceService, RoulettesurfaceService>();
            services.AddScoped<RoulettecoinsService, RoulettecoinsService>();
            services.AddScoped<RouletteBetoptionsService, RouletteBetoptionsService>();
            services.AddScoped<RouletteBetsService, RouletteBetsService>();
            services.AddScoped<RouletteitellisenseService, RouletteitellisenseService>();
            services.AddScoped<Motorsportracewayservice, Motorsportracewayservice>();

            services.AddServerSideBlazor()
                .AddHubOptions(options => options.MaximumReceiveMessageSize = 512 * 1024);

            services.AddCors(option =>
            {
                option.AddPolicy("wasmcorspolicy", (builder) =>
                {
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                           .SetIsOriginAllowed(isOriginAllowed => true)
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            services.AddSignalR()
                .AddHubOptions<ChatHub>(options =>
                {
                    options.EnableDetailedErrors = true;
                    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                    options.MaximumReceiveMessageSize = Int64.MaxValue;
                    options.StreamBufferCapacity = Int32.MaxValue;
                })
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.WriteIndented = false;
                    options.PayloadSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
                    options.PayloadSerializerOptions.AllowTrailingCommas = true;
                    options.PayloadSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                    options.PayloadSerializerOptions.DefaultBufferSize = 4096;
                    options.PayloadSerializerOptions.MaxDepth = 41;
                    options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseTenantResolution();
            app.UseBlazorFrameworkFiles();
            app.UseRouting();
            app.UseCors("wasmcorspolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/1/api/chathub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                    options.ApplicationMaxBufferSize = Int64.MaxValue;
                    options.TransportMaxBufferSize = Int64.MaxValue;
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(10);
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(10);
                });
            });
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {

        }

    }

    public static class WasmChatServiceCollectionExtensions
    {
        internal static IServiceCollection TryAddHttpClientWithAuthenticationCookie(this IServiceCollection services)
        {

            var httpClientService = services.FirstOrDefault(x => x.ServiceType == typeof(HttpClient));
            if (httpClientService != null)
            {
                services.Remove(httpClientService);
            }

            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                services.AddScoped(s =>
                {
                    // creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                    var navigationManager = s.GetRequiredService<NavigationManager>();
                    var client = new HttpClient(new HttpClientHandler { UseCookies = false });
                    client.BaseAddress = new Uri(navigationManager.Uri);
                    client.Timeout = TimeSpan.FromHours(20);

                    // set the cookies to allow HttpClient API calls to be authenticated
                    var httpContextAccessor = s.GetRequiredService<IHttpContextAccessor>();
                    foreach (var cookie in httpContextAccessor.HttpContext.Request.Cookies)
                    {
                        client.DefaultRequestHeaders.Add("Cookie", cookie.Key + "=" + cookie.Value);
                    }

                    return client;
                });
            }

            return services;
        }
    }

}
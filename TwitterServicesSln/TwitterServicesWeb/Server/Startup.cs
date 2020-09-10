using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using PTI.TwitterServices.Configuration;
using PTI.TwitterServices.Services;
using Microsoft.Extensions.Logging;
using PTI.BackgroundServices;
using PTI.BackgroundServices.SignalR;

namespace TwitterServicesWeb.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddLogging();
            services.AddTransient<ILogger, Logger<BaseTwitterService>>();
            services.AddTransient<ILogger, Logger<FakeFollowersTwitterService>>();
            services.AddTransient<ILogger, Logger<TwitterBackgroundService>>();
            var baseTwitterServiceConfiguration = 
                Configuration.GetSection("BaseTwitterServiceConfiguration").Get<BaseTwitterServiceConfiguration>();
            services.AddSingleton<BaseTwitterServiceConfiguration>(baseTwitterServiceConfiguration);
            services.AddTransient<BaseTwitterService>();
            services.AddTransient<FakeFollowersTwitterService>();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddHostedService<TwitterBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<PossibleFakeFollowersHub>("/possiblefakefollowershub");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}

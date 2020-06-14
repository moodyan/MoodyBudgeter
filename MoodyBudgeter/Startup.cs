using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.GoogleAuth;
using MoodyBudgeter.Utility.Clients.RestRequester;
using MoodyBudgeter.Utility.Lock;
using System.Net.Http;

namespace MoodyBudgeter
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGoogleOAuthClient, GoogleOAuthClient>();
            services.AddSingleton<IRestRequester, RestRequester>();
            services.AddSingleton<IEnvironmentRequester, EnvironmentRequester>();
            services.AddSingleton<IBudgeterCache, BudgeterCache>();
            services.AddSingleton<HttpClient, HttpClient>();
            services.AddSingleton<IBudgeterLock, BudgeterLock>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}

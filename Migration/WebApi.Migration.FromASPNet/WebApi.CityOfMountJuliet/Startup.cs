using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApi.CityOfMountJuliet.Models.Data;
using WebApi.CityOfMountJuliet.Models.Library;

namespace WebApi.CityOfMountJuliet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // ko cần này vì ko insert trực tiếp vào DB
            //services.AddDbContext<dbBloggingKPContext>(options =>
            //        options.UseSqlServer(Configuration.GetConnectionString("ConnectionString")));

            // cách lấy 1 vài section 
            //services.Configure<EmailOptions>(Configuration.GetSection("Email"));
            //services.Configure<KafkaOptions>(Configuration.GetSection("Kafka"));

            /* Sharable Static Properties
            // giống như xài HttpContext.Current mọi nơi miễn có using System.Web là đc
            // ASP net core xài IHttpContextAccessor # HttpContext.Current
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPrincipal>(
            provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
            */
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddWebApiConventions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

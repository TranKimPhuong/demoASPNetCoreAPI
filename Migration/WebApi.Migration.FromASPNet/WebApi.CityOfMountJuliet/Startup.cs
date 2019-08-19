using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
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
using WebApi.CityOfMountJuliet.Services.MasterData;
using WebApi.CityOfMountJuliet.Services.Payment;
using WebApi.CommonCore.Extensions;

namespace WebApi.CityOfMountJuliet
{
    public class Startup
    {
        public IConfiguration _configuration { get; set; }
        public IHostingEnvironment _environment { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // *If* you need access to generic IConfiguration this is **required**
            services.AddSingleton(_configuration);

            // for IHostingEnvironment

            // for HttpContext
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Or you can also register as follows
            //services.AddHttpContextAccessor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            /* 1. Calling these methods first ensures that exceptions are caught in any of the middleware 
             *   components that follow:
             *   
             *   app.UseDeveloperExceptionPage();
             *   app.UseDatabaseErrorPage();
             *              => used in development to catch run-time exceptions
            *    app.UseExceptionHandler("/Home/Error"); //used in production for run-time exceptions
            */
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error"); 
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(); // used in production to enable HSTS (HTTP Strict Transport Security Protocol) and enforce HTTPS
            }

            //app.UseHttpsRedirection(); //forces HTTP calls to automatically redirect to equivalent HTTPS addresses.

            //app.UseDefaultFiles();
            //app.UseStaticFiles(); //used to enable static files, such as HTML, JavaScript, CSS and graphics files

            //app.UseCookiePolicy(): used to enforce cookie policy and display GDPR-friendly messaging
            //app.UseAuthentication(): used to enable authentication and then subsequently allow authorization.
            //app. UseSession(): manually added to the Startup file to enable the Session middleware.

            // use khi nào
            //app.UseMapMiddleware();

            //init ham convert CountryCodeConverter
            //var aa = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            //CountryCodeConverter.Init(aa);

            app.UseMvc(); //enables the use of MVC in your web application, with the ability to customize routes for your MVC application and set other options.

            //DebugTest();
        }
        public static void DebugTest()
        {
            var inputpath = @"E:\SaaS\Input for debug code\city of mount juliet\Check Run 5-21-19";
            var inputfile = File.ReadAllBytes(inputpath);
            var conversion = new PaymentPsTool();
            var result = conversion.ProcessDataFile(inputfile);
            CountryCodeConverter.ConvertToThreeLetterISORegionName("");

            File.WriteAllText(@"E:\SaaS\Input for debug code\city of mount juliet\Payment.txt", result.ToString());

            //var inputpath = @"E:\SaaS\Input for debug code\city of mount juliet\CityofMountJuliet_Vendors.xlsx";
            //var inputfile = File.ReadAllBytes(inputpath);


            //var conversion = new MasterDataPsTool();
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var result = conversion.ProcessDataFile(inputfile);
            //File.WriteAllText(@"E:\SaaS\Input for debug code\city of mount juliet\Master.txt", result.ToString());
        }
    }
}

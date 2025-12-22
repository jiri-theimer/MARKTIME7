
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Telerik.Reporting.Services;


namespace UI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private string _wwwrootfolder;

        public Startup(IWebHostEnvironment env)
        {
            _wwwrootfolder = env.WebRootPath;

            Configuration = new ConfigurationBuilder()
                 .SetBasePath(env.ContentRootPath)
                 .AddJsonFile("appsettings.json", false, true)
                 .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                 .Build();
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Configuration.GetSection("Authentication")["KeyPath"]))
                .SetApplicationName(Configuration.GetSection("Authentication")["AppName"]);



            services.AddAuthentication("Identity.Application")
                 .AddCookie("Identity.Application", config =>
                 {
                     config.Cookie.HttpOnly = true;
                     config.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                     config.Cookie.SameSite = SameSiteMode.Strict;
                     config.SlidingExpiration = true;
                     config.ExpireTimeSpan = TimeSpan.FromHours(24);
                     config.Cookie.Name = Configuration.GetSection("Authentication")["CookieName"];
                     config.Cookie.Path = "/";
                     if (!string.IsNullOrEmpty(Configuration.GetSection("Authentication")["Domain"]))
                     {
                         config.Cookie.Domain = Configuration.GetSection("Authentication")["Domain"];         //pokud je nastavena doména, tak funguje jako SSO, tj. sdílení cookie pøihlášeného uživatele
                     }

                     config.ReturnUrlParameter = "returnurl";
                     config.LoginPath = "/Login/UserLogin";
                 });




            //services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            //{
            //    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
            //    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
            //});

            //aby se do html zapisovali originál unicode znaky:
            services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options => { options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All); });

            services.AddMvc(options => options.EnableEndpointRouting = false);  //nutné kvùli podpoøe routingu Areas: Mobile
            services.AddControllers();      //kvùli telerik reporting
            services.AddControllersWithViews();
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;      //kvùli telerik reporting
            });

            services.AddHttpClient();       //kvùli práci s httpclient v rámci IHttpClientFactory

            services.AddRazorPages().AddNewtonsoftJson();   //kvùli telerik reporting


            services.AddSingleton<BL.Singleton.RunningApp>(p => new BL.Singleton.RunningApp()
            {
                WwwRootFolder = _wwwrootfolder
            });



            services.AddSingleton<BL.Singleton.TheEntitiesProvider>();
            services.AddSingleton<BL.Singleton.TheTranslator>();
            services.AddSingleton<BL.TheColumnsProvider>();
            services.AddSingleton<BL.Singleton.ThePeriodProvider>();
            services.AddSingleton<BL.Singleton.BackgroundWorkerQueue>();




            //Služba pro TELERIK REPORTING:
            services.TryAddSingleton<IReportServiceConfiguration>(sp =>
            new ReportServiceConfiguration
            {
                ReportingEngineConfiguration = ConfigurationHelper.ResolveConfiguration(sp.GetService<IWebHostEnvironment>()),
                HostAppId = $"ReportViewer{Configuration.GetSection("App")["Name"]}",
                Storage = new Telerik.Reporting.Cache.File.FileStorage()
            });


            services.AddScoped<BO.RunningUser, BO.RunningUser>();
            services.AddScoped<BL.Factory, BL.Factory>();




            if (Configuration.GetSection("App").GetValue<Boolean>("IsRobot"))
            {
                services.AddHostedService<UI.Code.LongRunningService>();   //pohon pro robota na pozadí                
                services.AddHostedService<UI.Code.RobotRunner>();           //spouštìè robota                

            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseDeveloperExceptionPage();    //zjt: v rámci vývoje
            if (Configuration.GetSection("App")["NotUseHttpsRedirection"] != null && !Configuration.GetSection("App").GetValue<Boolean>("NotUseHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }


            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();    //user-identity funguje pouze s app.UseAuthentication
            app.UseAuthorization();

            app.UseRequestLocalization();

            var strCultureCode = Configuration.GetSection("App")["CultureCode"];
            if (string.IsNullOrEmpty(strCultureCode)) strCultureCode = "cs-CZ";
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(strCultureCode);
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(strCultureCode);



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); //kvùli teleri reporting

                //endpoints.MapAreaControllerRoute(
                //  name: "mobile",
                //  areaName: "Mobile",
                //  pattern: "Mobile/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Widgets}/{action=Index}/{id?}");



                endpoints.MapRazorPages();

            });



            ///upload složka _users pro NOTEPAD/PLUGINS
            string strUsersFolder = Path.Combine(Configuration.GetSection("Folders")["RootUpload"], "_users");
            if (!Path.Exists(strUsersFolder))
            {
                Directory.CreateDirectory(strUsersFolder);
            }
            app.UseStaticFiles(new StaticFileOptions
            {

                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Configuration.GetSection("Folders")["RootUpload"], "_users")),
                RequestPath = "/_users"
            });



            

            loggerFactory.AddFile("Logs/info-{Date}.log", LogLevel.Information);
            loggerFactory.AddFile("Logs/debug-{Date}.log", LogLevel.Debug);
            loggerFactory.AddFile("Logs/error-{Date}.log", LogLevel.Error);



        }


    }
}

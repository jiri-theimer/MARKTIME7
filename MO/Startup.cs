using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Globalization;
using Telerik.Reporting.Services;


namespace MO
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
            // SSO s UI: shared KeyPath + AppName + CookieName (a Domain v appsettings) zajistí sdílenou auth cookie.
            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Configuration.GetSection("Authentication")["KeyPath"]))
                .SetApplicationName(Configuration.GetSection("Authentication")["AppName"]);


            services.AddAuthentication("Identity.Application")
                 .AddCookie("Identity.Application", config =>
                 {
                     config.Cookie.HttpOnly = true;
                     config.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                     // Lax (ne Strict): Strict by cookie odmítl poslat při příchodu "zvenčí" appky
                     // (odkaz z e-mailu, QR kód, ikona na ploše, SSO redirect z MO...) - i kdyby
                     // uživatel byl reálně přihlášený, takový požadavek by ho poslal na Login.
                     // Lax pořád chrání proti CSRF (cookie se neposílá u cross-site POST/iframe),
                     // ale funguje i při "vnější" navigaci na stránku.
                     config.Cookie.SameSite = SameSiteMode.Lax;
                     config.SlidingExpiration = true;
                     config.ExpireTimeSpan = TimeSpan.FromHours(24);
                     config.Cookie.Name = Configuration.GetSection("Authentication")["CookieName"];
                     config.Cookie.Path = "/";
                     if (!string.IsNullOrEmpty(Configuration.GetSection("Authentication")["Domain"]))
                     {
                         config.Cookie.Domain = Configuration.GetSection("Authentication")["Domain"];   // pokud je nastavena doména, funguje jako SSO (sdílení cookie přihlášeného uživatele)
                     }

                     config.ReturnUrlParameter = "returnurl";
                     config.LoginPath = "/Login/UserLogin";
                 });


            // unicode znaky v HTML
            services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);
            });

            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddControllers();      // kvůli Telerik Reporting
            services.AddControllersWithViews();
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;  // kvůli Telerik Reporting
            });

            services.AddHttpClient();

            services.AddRazorPages().AddNewtonsoftJson();   // kvůli Telerik Reporting


            services.AddSingleton<BL.Singleton.RunningApp>(p => new BL.Singleton.RunningApp()
            {
                WwwRootFolder = _wwwrootfolder
            });

            services.AddSingleton<BL.Singleton.TheEntitiesProvider>();
            services.AddSingleton<BL.Singleton.TheTranslator>();
            services.AddSingleton<BL.TheColumnsProvider>();
            services.AddSingleton<BL.Singleton.ThePeriodProvider>();
            services.AddSingleton<BL.Singleton.BackgroundWorkerQueue>();

            // VIES služba
            services.AddHttpClient<BL.Code.IViesClient, BL.Code.ViesClient>(client =>
            {
                client.BaseAddress = new Uri("http://ec.europa.eu/taxation_customs/vies/services/");
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            var strDefaultCultureCode = Configuration.GetSection("App")["CultureCode"];
            if (string.IsNullOrEmpty(strDefaultCultureCode)) strDefaultCultureCode = "cs-CZ";

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "cs-CZ", "en-US", "sk-SK" }
                    .Select(x => new CultureInfo(x))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture("cs-CZ");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                var cookieProvider = new CookieRequestCultureProvider();
                var headerProvider = new AcceptLanguageHeaderRequestCultureProvider();

                options.RequestCultureProviders = new IRequestCultureProvider[]
                {
                    new CustomRequestCultureProvider(async context =>
                    {
                        // 1) zjisti UI kulturu (cookie -> header -> default)
                        ProviderCultureResult? result =
                            await cookieProvider.DetermineProviderCultureResult(context)
                            ?? await headerProvider.DetermineProviderCultureResult(context);

                        var ui = result?.UICultures?.FirstOrDefault()
                                 ?? options.DefaultRequestCulture.UICulture.Name;

                        // 2) formátování vždy default
                        return new ProviderCultureResult(culture: strDefaultCultureCode, uiCulture: ui);
                    })
                };
            });


            // Služba pro TELERIK REPORTING:
            services.TryAddSingleton<IReportServiceConfiguration>(sp =>
                new ReportServiceConfiguration
                {
                    ReportingEngineConfiguration = ConfigurationHelper.ResolveConfiguration(sp.GetService<IWebHostEnvironment>()),
                    HostAppId = $"ReportViewer{Configuration.GetSection("App")["Name"]}",
                    Storage = new Telerik.Reporting.Cache.File.FileStorage()
                });


            services.AddScoped<BO.RunningUser, BO.RunningUser>();
            services.AddScoped<BL.Factory, BL.Factory>();
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

            if (Configuration.GetSection("App")["NotUseHttpsRedirection"] != null
                && !Configuration.GetSection("App").GetValue<Boolean>("NotUseHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }


            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestLocalization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();     // kvůli Telerik Reporting

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });


            loggerFactory.AddFile("Logs/info-{Date}.log", LogLevel.Information);
            loggerFactory.AddFile("Logs/debug-{Date}.log", LogLevel.Debug);
            loggerFactory.AddFile("Logs/error-{Date}.log", LogLevel.Error);
        }
    }
}
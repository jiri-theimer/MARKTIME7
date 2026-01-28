using Bas;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddSingleton<Bas.Singleton.TheTranslator>();


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "cs-CZ", "en-US", "de-DE", "ru-RU", "sk-SK", "uk-UA" }.Select(x => new CultureInfo(x)).ToList();

    options.DefaultRequestCulture = new RequestCulture("cs-CZ");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new IRequestCultureProvider[]
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});


builder.Services.AddSingleton<BL.Singleton.RunningApp>(p => new BL.Singleton.RunningApp()
{
    WwwRootFolder = ""
});
builder.Services.AddSingleton<BL.Singleton.TheEntitiesProvider>();
builder.Services.AddSingleton<BL.Singleton.TheTranslator>();


builder.Services.AddScoped<PageService>();  //podpora translate pro csthml stránky 
builder.Services.AddScoped<BO.RunningUser, BO.RunningUser>();
builder.Services.AddScoped<BL.Factory, BL.Factory>();

//aby se do html zapisovali origin l unicode znaky:
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options => { options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All); });



var app = builder.Build();

var loc = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();      //jazyky
app.UseRequestLocalization(loc.Value);                                                  //jazyky

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}
else
{

    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


if (!Directory.Exists($"{app.Environment.ContentRootPath}\\Logs"))
{
    Directory.CreateDirectory($"{app.Environment.ContentRootPath}\\Logs");
}



var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

loggerFactory.AddFile("Logs/info-{Date}.log", LogLevel.Information);
loggerFactory.AddFile("Logs/debug-{Date}.log", LogLevel.Debug);
loggerFactory.AddFile("Logs/error-{Date}.log", LogLevel.Error);



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



namespace UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                    webBuilder.UseSetting("detailedErrors", "true");    //zajistí detail výpisu pøípadných chyb i zde na úvod v Program.cs a Startup.cs
                });
    }
}
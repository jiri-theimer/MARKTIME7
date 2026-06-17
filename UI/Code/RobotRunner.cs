using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

using System.Threading;

using System.Net.Http;
using System.Security.Policy;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace UI.Code
{
    public class RobotRunner: IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        private readonly BL.Singleton.RunningApp _app;       
        private readonly IHttpClientFactory _httpclientfactory; //client http
        private readonly IServer _server;
        public RobotRunner(BL.Singleton.RunningApp app, IHttpClientFactory hcf, IServer server)
        {
            _app = app;
            _server = server;
            _httpclientfactory = hcf;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            LogInfo("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(500));   //každých 500 sekund

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken stoppingToken)
        {
            LogInfo("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }


        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            if (_app == null)
            {
                LogInfo("_app is null!");
                return;
            }
            if (!_app.IsRobot)
            {
                LogInfo("[IsRobot] is FALSE!");
                return;
            }

            if (string.IsNullOrEmpty(_app.RobotUrl))
            {
                var addresses = _server.Features.Get<IServerAddressesFeature>().Addresses;  //všechny aplikační localhost url
                if (addresses != null && addresses.Count() > 0)
                {
                    LogInfo("Robot url: " + addresses.First().ToString() + "/Robot");

                    string s=RunRobotUrl(_httpclientfactory.CreateClient(), addresses.First().ToString() + "/Robot").Result;   //spuštění controlleru robota (RobotController)

                }
                else
                {
                    LogInfo("Robot url: NOT FOUND!!!!!");
                }
            }
            else
            {
                LogInfo("Robot url (načteno z appconfig.json): " + _app.RobotUrl);

                string s=RunRobotUrl(_httpclientfactory.CreateClient(), _app.RobotUrl).Result;   //spuštění controlleru robota (RobotController)
            }
            





        }

        private async Task<string> RunRobotUrl(HttpClient client,string url)
        {
            
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                try
                {
                    
                    var response = await client.SendAsync(request);

                    var strRet = await response.Content.ReadAsStringAsync();
                    return strRet;

                }
                catch (Exception ex)
                {
                    LogInfo(ex.Message);
                    return null;
                }


            }
        }

        private void LogInfo(string strMessage)
        {
            var strPath = string.Format("{0}\\robot-runner-{1}.log", _app.LogFolder, DateTime.Now.ToString("yyyy.MM.dd"));
            try
            {
                System.IO.File.AppendAllLines(strPath, new List<string>() { "", DateTime.Now.ToString() + ": ", strMessage });
            }
            catch
            {

            }

        }

    }
}

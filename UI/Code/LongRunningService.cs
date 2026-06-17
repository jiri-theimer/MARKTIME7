using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace UI.Code
{
    public class LongRunningService : BackgroundService
    {
        private readonly BL.Singleton.BackgroundWorkerQueue queue;

        public LongRunningService(BL.Singleton.BackgroundWorkerQueue queue)
        {
            this.queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await queue.DequeueAsync(stoppingToken);

                await workItem(stoppingToken);

            }
        }
    }

}

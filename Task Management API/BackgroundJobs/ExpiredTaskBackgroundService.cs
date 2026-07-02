using Task_Management_API.Interfaces;

namespace Task_Management_API.BackgroundJobs
{
    public class ExpiredTaskBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredTaskBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public ExpiredTaskBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredTaskBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiredTaskBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                    await taskService.MarkOverdueTasksAsExpiredAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while marking overdue tasks as expired.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("ExpiredTaskBackgroundService stopped.");
        }
    }
}

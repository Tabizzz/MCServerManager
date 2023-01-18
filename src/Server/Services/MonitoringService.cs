using MCServerManager.Server.Abstractions;
namespace MCServerManager.Server.Services;

public class MonitoringService<T> : BackgroundService where T : IAsyncRunnable
{
	readonly ILogger<MonitoringService<T>> _logger;

	readonly T _toRun;
	
	public MonitoringService(ILogger<MonitoringService<T>> logger, T toRun)
	{
		_logger = logger;
		_toRun = toRun;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Starting {Name} monitoring service", typeof(T).Name);

		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(_toRun.MonitorInterval, stoppingToken);
				await _toRun.Run(stoppingToken);
			}
		}
		catch (TaskCanceledException)
		{ }
	}
}
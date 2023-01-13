namespace WebServerManager.Server.Abstractions;

public interface IAsyncRunnable : IAsyncDisposable
{
	TimeSpan MonitorInterval { get; }
	
	Task Run(CancellationToken cancellationToken);
}
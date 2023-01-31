using MCServerManager.Desktop.Models;
using MessagePipe;
using MudBlazor;
namespace MCServerManager.Desktop.Services;

public class TaskService : List<BackgroundTask>
{
	static readonly object Lock = new();

	ISnackbar _snackbar = null!;

	readonly IAsyncPublisher<BackgroundTask> _publisher;

	public TaskService(IAsyncPublisher<BackgroundTask> publisher)
	{
		_publisher = publisher;
	}
	
	public void Init(ISnackbar snackbar)
	{
		_snackbar = snackbar;
	}
	
	public void Create(BackgroundTask task)
	{
		lock (Lock)
		{
			task.Task = task.TaskCreator(CreateRunningTask(task));
			task.Task.ContinueWith(_ => Finalize(task));
			Add(task);
			_publisher.PublishAsync(task);
		}
	}

	RunningBackgroundTask CreateRunningTask(BackgroundTask task)
	{
		return new()
		{
			BackgroundTask = task,
			Update = () =>
			{
				_publisher.PublishAsync(task);
			}
		};
	}

	void Finalize(BackgroundTask task)
	{
		lock (Lock)
		{
			if (task.CompletionMessage is not null)
			{
				_snackbar.Add(task.CompletionMessage, task.CompletionSeverity, key: task.Key ?? $"BackgroundTask#{task.Task?.Id}");
			}
			Remove(task);
			_publisher.PublishAsync(task);
		}
	}
}
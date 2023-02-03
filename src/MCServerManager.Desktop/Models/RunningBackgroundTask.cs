using MessagePipe;
namespace MCServerManager.Desktop.Models;

/// <summary>
/// Represent a task that is running on the background
/// </summary>
public class RunningBackgroundTask
{
	/// <summary>
	/// The actual task.
	/// </summary>
	public BackgroundTask BackgroundTask { get; }

	DateTime _lastUpdate;

	readonly IAsyncPublisher<BackgroundTask> _publisher;

	public RunningBackgroundTask(BackgroundTask task, IAsyncPublisher<BackgroundTask> publisher)
	{
		BackgroundTask = task;
		_lastUpdate = DateTime.Now;
		_publisher = publisher;
	}

	/// <summary>
	/// Update the progress of the task.
	/// </summary>
	/// <param name="progress">New progress, a value between 0 and 100 or -1 to indeterminate.</param>
	public void UpdateProgress(float progress)
	{
		BackgroundTask.Progress = progress;
		Update();
	}

	public void Update()
	{
		var passedTime = DateTime.Now - _lastUpdate;
		if (passedTime.Milliseconds > 10)
		{
			_lastUpdate = DateTime.Now;
			_publisher.Publish(BackgroundTask);
		}
	}
}
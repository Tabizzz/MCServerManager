namespace MCServerManager.Desktop.Models;

/// <summary>
/// Represent a task that is running on the background
/// </summary>
public class RunningBackgroundTask
{
	/// <summary>
	/// The actual task.
	/// </summary>
	public required BackgroundTask BackgroundTask { get; init; }
	
	/// <summary>
	/// An action used to update the task in the UI, should be called after ny modification to the task.
	/// </summary>
	public required Action Update { get; init; }

	/// <summary>
	/// Update the progress of the task.
	/// </summary>
	/// <param name="progress">New progress, a value between 0 and 100 or -1 to indeterminate.</param>
	public void UpdateProgress(float progress)
	{
		BackgroundTask.Progress = progress;
		Update();
	}
}
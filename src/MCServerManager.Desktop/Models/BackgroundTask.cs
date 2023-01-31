using MudBlazor;
namespace MCServerManager.Desktop.Models;

public class BackgroundTask
{
	public required Func<RunningBackgroundTask, Task> TaskCreator { get; init; }
	
	public Action<BackgroundTask>? Cancel { get; set; }

	internal Task? Task { get; set; }
	
	public string? Key { get; init; }
	
	public string? CompletionMessage { get; set; }

	public Severity CompletionSeverity { get; set; }
	
	public required string Tittle { get; init; }
	
	public string? Description { get; set; }

	public float Progress { get; set; } = -1;
}
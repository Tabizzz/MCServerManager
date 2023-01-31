using MCServerManager.Desktop.Models;
using MessagePipe;
using Microsoft.AspNetCore.Components;
namespace MCServerManager.Desktop.Components;

public partial class TaskListItem
{
	bool _isOpen;
	
	[Parameter] public BackgroundTask Task { get; set; } = null!;
}
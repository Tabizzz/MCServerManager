using MCServerManager.Desktop.Models;
using MessagePipe;
namespace MCServerManager.Desktop.Components;

public partial class TaskList : IDisposable
{
	bool _open;

	IDisposable _disposable = null!;

	protected override void OnInitialized()
	{
		_disposable = Subscriber.Subscribe(OnTaskUpdate);
	}

	public async ValueTask OnTaskUpdate(BackgroundTask task, CancellationToken token)
	{
		await InvokeAsync(StateHasChanged);
	}

	public void Dispose()
	{
		_disposable.Dispose();
	}
}
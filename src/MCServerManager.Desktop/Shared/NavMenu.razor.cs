using MCServerManager.Desktop.Models;
using MessagePipe;
namespace MCServerManager.Desktop.Shared;

public partial class NavMenu
{
	IDisposable _disposable = null!;
	
	protected override void OnInitialized()
	{
		_disposable = SftpSubscriber.Subscribe(HandleAsync);
	}

	public void Dispose()
	{
		_disposable.Dispose();
	}

	public ValueTask HandleAsync(SftpCredentials message, CancellationToken cancellationToken)
	{
		StateHasChanged();
		return ValueTask.CompletedTask;
	}
}
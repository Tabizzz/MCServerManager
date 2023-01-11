using MessagePipe;
using WebServerManager.Shared;
namespace WebServerManager.Client.Shared;

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
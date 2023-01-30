using MCServerManager.Desktop.Components.Dialogs;
using MCServerManager.Desktop.Models;
using MessagePipe;
using MudBlazor;
namespace MCServerManager.Desktop.Pages;

public partial class Servers : IDisposable
{
	IDisposable _disposable = null!;
	
	protected override void OnInitialized()
	{
		_disposable = DisposableBag.Create(Subscriber.Subscribe(OnServerChange));
	}

	ValueTask OnServerChange(MCServer server, CancellationToken token)
	{
		StateHasChanged();
		return ValueTask.CompletedTask;
	}
	
	async Task OpenCreateServerDialog()
	{
		var options = new DialogOptions()
		{
			Position = DialogPosition.Center,
			CloseOnEscapeKey = false,
			CloseButton = false,
			DisableBackdropClick = true,
			MaxWidth = MaxWidth.Small
		};
		var dialog = await DialogService.ShowAsync<CreateServerDialog>("Add Server - General Info", options);
		var result = await dialog.Result;
		if (!result.Canceled)
		{
			if(ServerManager[(Guid)result.Data] is { } server)
				Snackbar.Add($"Server <b>{server.Name}</b> has been added", Severity.Success);
			await Storage.Save(JsRuntime);
		}
		StateHasChanged();
	}
	
	public void Dispose()
	{
		_disposable.Dispose();
	}
}
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

	async Task DeleteServer(MCServer server)
	{
		var result = await DialogService.ShowMessageBox(
			"Warning", 
			"Deleting can not be undone!", 
			yesText:"Delete!", cancelText:"Cancel");
		
		if (result is true)
		{
			TaskService.Create(new ()
			{
				Tittle = $"Deleting server {server.Name}",
				TaskCreator = DeleteServerCore(server)
			});
		}
	}

	Func<RunningBackgroundTask, Task> DeleteServerCore(MCServer server) => async task =>
	{
		task.BackgroundTask.CompletionSeverity = Severity.Info;
		task.BackgroundTask.CompletionMessage = $"Server <b>{server.Name}</b> has been deleted";
		ServerManager.Remove(server.Id);
		ConnectionsManager.DeleteConnection(server.Id);
		ServerFactory.Destroy(ServerFactory.Entries.First(t => t.Label == server.Name));
		await Storage.Save(JsRuntime);
	};
}
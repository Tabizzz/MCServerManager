using MCServerManager.Desktop.Components.Dialogs;
using Microsoft.JSInterop;
using MudBlazor;
namespace MCServerManager.Desktop.Pages;

public partial class Servers
{

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
			await ServerManager.Save(JsRuntime);
		}
		StateHasChanged();
	}
}
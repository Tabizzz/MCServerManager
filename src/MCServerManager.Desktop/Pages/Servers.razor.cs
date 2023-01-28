using MCServerManager.Desktop.Components.Dialogs;
using MudBlazor;
namespace MCServerManager.Desktop.Pages;

public partial class Servers
{

	async Task OpenCreateServerDialog()
	{
		var options = new DialogOptions()
		{
			Position = DialogPosition.Center,
			CloseOnEscapeKey = true,
			DisableBackdropClick = true,
			MaxWidth = MaxWidth.Small
		};
		var dialog = await DialogService.ShowAsync<CreateServerDialog>("Add Server - General Info", options);
	}
}
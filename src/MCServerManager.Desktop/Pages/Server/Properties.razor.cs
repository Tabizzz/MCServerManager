using System.Net;
using MCServerManager.Desktop.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace MCServerManager.Desktop.Pages.Server;

public partial class Properties
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	string _filter = null!;

	bool _docs;

	bool _saving;

	protected override async Task OnInitializedAsync()
	{
		await PropertiesService.ReadProperties();
	}
	
	async Task SaveFile()
	{
		if(ServerManager.CurrentServer is null) return;

		_saving = true;
		StateHasChanged();
		
		await Sftp.UpdateRawText("/server.properties", ServerManager.CurrentServer.Id, PropertiesService.WriteProperties());

		if (Sftp.StatusCode == HttpStatusCode.Accepted)
		{
			Snackbar.Add("File sucessfully saved", Severity.Success, key: "properties/fileSaved");
		}
		else
		{
			Snackbar.Add("Unable to save the file", Severity.Error, key: "properties/fileSavedError");
		}
		_saving = false;
		StateHasChanged();
	}

}
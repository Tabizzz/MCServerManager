using System.Net;
using System.Net.Http.Json;
using MCServerManager.Desktop.Shared;
using MCServerManager.Shared;
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
		Layout.RequireSftp = true;
		if (!CredentialService.SftpCredentials.IsValid) return;

		await PropertiesService.ReadProperties();
	}
	
	async Task SaveFile()
	{
		_saving = true;
		StateHasChanged();
		
		await Sftp.UpdateRawText("/server.properties", CredentialService.SftpCredentials, PropertiesService.WriteProperties());

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
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebServerManager.Client.Models;
using WebServerManager.Client.Shared;
using WebServerManager.Shared;
namespace WebServerManager.Client.Pages.Server;

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
		
		var ressponse = await Http.PostAsJsonAsync("Sftp/UpdateRawText", new UpdateTextFileRequest()
		{
			Content = PropertiesService.WriteProperties(),
			Path = "/server.properties",
			Credentials = CredentialService.SftpCredentials
		});

		if (ressponse.StatusCode == HttpStatusCode.Accepted)
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
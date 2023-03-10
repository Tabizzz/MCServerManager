using System.Net;
using System.Net.Http.Json;
using System.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace WebServerManager.Client.Dialogs;

public partial class FileCreateDialog
{
	[CascadingParameter]
	MudDialogInstance MudDialog { get; set; } = null!;

	public string FileName { get; set; } = "";

	string? _filenamecopy;

	[Parameter] public string Path { get; set; } = "/";
	
	[Parameter] public bool Directory { get; set; }

	bool _creating;
	bool _existing;

	async Task Submit()
	{
		if (!string.IsNullOrWhiteSpace(FileName))
		{
			_creating = true;
			_existing = false;
			_filenamecopy = FileName;
			StateHasChanged();
			
			var toSave = System.IO.Path.GetFullPath(FileName, Path);
			var urlEncode = HttpUtility.UrlEncode(toSave);
			var response = await Http.PostAsJsonAsync($"Sftp/CreateEmptyFile?path={urlEncode}&directory={Directory}", CredentialService.SftpCredentials);
			switch (response.StatusCode)
			{
				case HttpStatusCode.Accepted:
					MudDialog.Close(DialogResult.Ok(toSave));
					NavigationManager.NavigateTo((Directory ? "files" : "raw") + toSave);
					break;
				case HttpStatusCode.NoContent:
					_existing = true;
					break;
				default:
					MudDialog.Close(DialogResult.Cancel());
					break;
			}
			
			_creating = false;
			StateHasChanged();
		}
	}
}
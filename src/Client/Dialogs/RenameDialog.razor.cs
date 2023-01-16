using System.Net;
using System.Net.Http.Json;
using System.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebServerManager.Client.Services;
namespace WebServerManager.Client.Dialogs;

public partial class RenameDialog
{
	[CascadingParameter]
	MudDialogInstance MudDialog { get; set; } = null!;
	
	public string NewName { get; set; } = "";
	
	string? _filenamecopy;

	[Parameter] public string Path { get; set; } = "/";
	
	[Parameter] public string Src { get; set; } = "/";
	
	[Parameter] public bool Directory { get; set; }

	public bool IsRename => 
		string.IsNullOrWhiteSpace(NewName) ||
		System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(NewName, Path)) == Path;

	bool _renaming;
	bool _existing;

	async Task Submit()
	{
		if (!string.IsNullOrWhiteSpace(NewName))
		{
			_renaming = true;
			_existing = false;
			_filenamecopy = NewName;
			StateHasChanged();
			
			var toSave = HttpUtility.UrlEncode(Src);;
			var dest = System.IO.Path.GetFullPath(NewName, Path);
			var urlEncode = HttpUtility.UrlEncode(dest);
			var response = await Http.PostAsJsonAsync($"Sftp/RenameFile?src={toSave}&dest={urlEncode}", CredentialService.SftpCredentials);
			switch (response.StatusCode)
			{
				case HttpStatusCode.Accepted:
					MudDialog.Close(DialogResult.Ok(true));
					break;
				case HttpStatusCode.NoContent:
					_existing = true;
					break;
				default:
					MudDialog.Close(DialogResult.Cancel());
					break;
			}
			
			_renaming = false;
			StateHasChanged();
		}
	}
}
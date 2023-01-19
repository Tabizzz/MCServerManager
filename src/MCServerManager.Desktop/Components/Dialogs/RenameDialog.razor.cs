using System.Net;
using System.Net.Http.Json;
using System.Web;
using MCServerManager.Desktop.Utils;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace MCServerManager.Desktop.Components.Dialogs;

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
		System.IO.Path.GetDirectoryName(PathUtils.GetFullPath(NewName, Path)) == Path;

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
			
			await Sftp.RenameFile(CredentialService.SftpCredentials, Src, PathUtils.GetFullPath(Path, NewName));
			switch (Sftp.StatusCode)
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
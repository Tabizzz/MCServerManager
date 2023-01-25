using MCServerManager.Desktop.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace MCServerManager.Desktop.Components.Dialogs;

public partial class CreateServerDialog
{
	[CascadingParameter]
	MudDialogInstance MudDialog { get; set; } = null!;

	readonly MCServer _server = new(Guid.NewGuid());

	SftpCredentials _sftp = new();

	byte _page;
	
	void Submit()
	{
		_page++;
	}

	void Back()
	{
		_page--;
	}
}
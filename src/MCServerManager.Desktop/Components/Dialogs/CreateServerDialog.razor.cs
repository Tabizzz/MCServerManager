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

	bool _validating;

	byte _phase;

	string? _error;

	bool _valid;
	
	async Task Submit()
	{
		if(_page < 2)
			_page++;
		_error = null;
		UpdateTittle();

		if (_page == 2)
		{
			_validating = true;
			_phase = 0;
			MudDialog.Options.CloseButton = false;
			MudDialog.Options.CloseOnEscapeKey = false;
			MudDialog.SetOptions(MudDialog.Options);
			StateHasChanged();

			var server = ServerFactory.Make(_server.Ip, _server.Port, false, _server.Name);
			
			await server.Updater.Ping(10);
			var last = server.Updater.GetLatestServerInfo(true);
			_server.Status = last;
			_phase++;
			StateHasChanged();
			

		}
	}

	void Back()
	{
		_page--;
		_error = null;
		UpdateTittle();
	}

	void UpdateTittle()
	{
		switch (_page)
		{
			case 0:
				MudDialog.SetTitle("Add Server - General Info");
				break;
			case 1:
				MudDialog.SetTitle("Add Server - Sftp Credentials");
				break;
			case 2:
				MudDialog.SetTitle("Add Server - Validation");
				break;
		}
	}

	void ValidateInputs()
	{
		_valid = true;
		
		if (_page == 0)
		{
			if (string.IsNullOrWhiteSpace(_server.Ip)) _valid = false;
			if (string.IsNullOrWhiteSpace(_server.Name)) _valid = false;
		}
	}
}
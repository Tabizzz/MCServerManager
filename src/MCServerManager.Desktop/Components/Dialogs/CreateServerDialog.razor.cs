using MCServerManager.Desktop.Models;
using MCServerManager.Desktop.Utils.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace MCServerManager.Desktop.Components.Dialogs;

public partial class CreateServerDialog
{
	[CascadingParameter]
	MudDialogInstance MudDialog { get; set; } = null!;

	readonly MCServer _server = new(Guid.NewGuid());

	readonly SftpCredentials _sftp = new();

	byte _page;

	bool _validating;

	byte _phase;

	string? _error;

	bool _valid;

	async Task Submit()
	{
		if(_page < 3)
			_page++;
		if (_page == 3)
		{
			MudDialog.Close(DialogResult.Ok(_server.Id));
			return;
		}
		ValidateInputs();
		_error = null;
		UpdateTittle();

		if (_page == 2)
		{
			_validating = true;
			_phase = 0;
			StateHasChanged();

			var server = ServerFactory.MakeOrGet(_server.Ip, _server.Port, _server.Name);
			
			await server.Updater.Ping(10);
			var last = server.Updater.GetLatestServerInfo(true);
			_server.Status = last;
			_phase++;
			StateHasChanged();

			try
			{
				_server.Sftp = _sftp;
				ServerManager.Register(_server);
				var value = await ConnectionsManager.CreateConnection(_server.Id, true);
				if (value is null)
					throw new("Unable to register server, no valid credentials");
			}
			catch (Exception e)
			{
				_server.Sftp = new();
				ServerManager.Remove(_server.Id);
				ServerFactory.Destroy(server);
				_error = e.Message;
				_page = 1;
				StateHasChanged();
				return;
			}
			finally
			{
				_validating = false;
				UpdateTittle();
			}
			if (_server.Status is null)
				_error = $"It has not been possible to make a ping to the server ({_server.Ip}:{_server.Port}), make sure that the IP and the port" +
				         " are correct, if they are correct and the server is offline ignores this message";
			
		}
	}

	void Back()
	{
		_page--;
		ValidateInputs();
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
		else if (_page == 1)
		{
			if (string.IsNullOrWhiteSpace(_sftp.Host)) _valid = false;
			if (string.IsNullOrWhiteSpace(_sftp.User)) _valid = false;
		}
	}

	public async Task Cancel()
	{
		var server = ServerFactory.Entries.FirstOrDefault(s => s.Updater.Address == _server.Ip && s.Updater.Port == _server.Port);
		if (server is not null)
			ServerFactory.Destroy(server);
		ServerManager.Remove(_server.Id);
		ConnectionsManager.DeleteConnection(_server.Id);
		MudDialog.Cancel();
	}
}
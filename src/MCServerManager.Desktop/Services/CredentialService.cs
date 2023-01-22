using System.Net;
using Blazored.LocalStorage;
using MCServerManager.Desktop.Controllers;
using MCServerManager.Desktop.Models;
namespace MCServerManager.Desktop.Services;

public class CredentialService : IDisposable
{
	readonly ILocalStorageService _local;
	
	readonly AuthController _auth;

	public bool Loaded { get; private set; }
	
	public SftpCredentials SftpCredentials { get; private set; }

	public CredentialService(ILocalStorageService local, AuthController auth)
	{
		_local = local; 
		_auth = auth;
		SftpCredentials = new();
	}

	public async Task Save()
	{
		var tosave = new SftpCredentials
		{
			SaveCredentials = SftpCredentials.SaveCredentials,
			ShareToken = SftpCredentials.ShareToken
		};
		if (tosave.ShareToken)
		{
			tosave.Token = SftpCredentials.Token;
		}
		if (tosave.SaveCredentials)
		{
			tosave.Host = SftpCredentials.Host;
			tosave.User = SftpCredentials.User;
			tosave.Port = SftpCredentials.Port;
			tosave.Password = SftpCredentials.Password;
		}
		await _local.SetItemAsync("SftpPreferences", tosave);
	}

	public async Task Load()
	{
		var data = await _local.GetItemAsync<SftpCredentials?>("SftpPreferences");
		Loaded = true;
		if(data is null) return;
		var toauth = new SftpCredentials();
		if (data is { ShareToken: true, Token: { } })
		{
			toauth.Token = data.Token;
			var response = _auth.GetSession(toauth);
			if (_auth.StatusCode == HttpStatusCode.Accepted && response is { Token: { } })
			{
				SftpCredentials = response;
				return;
			}
		}
		if (data.SaveCredentials)
		{
			var response = await _auth.Login(data);
			if (_auth.StatusCode == HttpStatusCode.Accepted && response is {Token:{ }})
			{
				SftpCredentials = (SftpCredentials)data.Clone();
				SftpCredentials.IsValid = true;
				SftpCredentials.Password = "";
				SftpCredentials.Token = response.Token;
			}
		}
		if (data.ShareToken && data.Token != SftpCredentials.Token)
		{
			data.Token = SftpCredentials.Token;
			await _local.SetItemAsync("SftpPreferences", data);
		}
	}

	public async Task Clear()
	{
		await _local.RemoveItemAsync("SftpPreferences");
	}

	public void Dispose()
	{
		if (SftpCredentials is { IsValid: true, ShareToken: false })
		{
			_auth.LogOut(SftpCredentials);
		}
	}
}
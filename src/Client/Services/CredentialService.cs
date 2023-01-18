using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using MCServerManager.Shared;
namespace MCServerManager.Client.Services;

public class CredentialService : IAsyncDisposable
{
	readonly ILocalStorageService _local;

	readonly HttpClient _client;

	public SftpCredentials SftpCredentials { get; private set; }

	public CredentialService(ILocalStorageService local, HttpClient client)
	{
		_local = local;
		_client = client;
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
		if(data is null) return;
		var toauth = new SftpCredentials();
		if (data is { ShareToken: true, Token: { } })
		{
			toauth.Token = data.Token;
			var response = await _client.PostAsJsonAsync("Auth/GetSession", toauth);
			if (response.StatusCode == HttpStatusCode.Accepted && await response.Content.ReadFromJsonAsync<SftpCredentials>() is { Token: not null} token)
			{
				SftpCredentials = token;
				return;
			}
		}
		if (data.SaveCredentials)
		{
			var response = await _client.PostAsJsonAsync("Auth/Login", data);
			var token = await response.Content.ReadFromJsonAsync<SftpCredentials>();
			if (response.StatusCode == HttpStatusCode.Accepted && token is not null)
			{
				SftpCredentials = (SftpCredentials)data.Clone();
				SftpCredentials.IsValid = true;
				SftpCredentials.Password = "";
				SftpCredentials.Token = token.Token;
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

	public async ValueTask DisposeAsync()
	{
		if (SftpCredentials is { IsValid: true, ShareToken: false })
		{
			await _client.PostAsJsonAsync("Auth/logout", SftpCredentials);
		}
	}
}
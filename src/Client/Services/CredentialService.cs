using Blazored.LocalStorage;
using WebServerManager.Shared;
namespace WebServerManager.Client.Services;

public class CredentialService
{
	readonly ILocalStorageService _local;

	readonly HttpClient _client;

	public SftpCredentials SftpCredentials { get; }

	public CredentialService(ILocalStorageService local, HttpClient client)
	{
		_local = local;
		_client = client;
		SftpCredentials = new();
		
	}
}
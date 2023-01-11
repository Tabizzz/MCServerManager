using Blazored.LocalStorage;
using WebServerManager.Shared;
namespace WebServerManager.Client.Services;

public class CredentialService
{
	public SftpCredentials SftpCredentials { get; }

	public CredentialService(ILocalStorageService local, HttpClient client)
	{
		SftpCredentials = new SftpCredentials();
		
	}
}
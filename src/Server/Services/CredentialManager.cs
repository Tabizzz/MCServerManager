using System.Collections.Concurrent;
using WebServerManager.Shared;
namespace WebServerManager.Server.Services;

// todo: better auth method?
public class CredentialManager
{
	IDictionary<string, SftpCredentials> CredentialsMap = new ConcurrentDictionary<string, SftpCredentials>();
	
	public string Login(SftpCredentials credentials)
	{
		var dev = Guid.NewGuid().ToString();
		CredentialsMap.Add(dev, credentials);
		return dev;
	}
}
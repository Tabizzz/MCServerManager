using System.Collections.Concurrent;
using WebServerManager.Shared;
namespace WebServerManager.Server.Services;

// todo: better auth method?
public class CredentialManager
{
	readonly IDictionary<string, SftpCredentials> _credentialsMap = new ConcurrentDictionary<string, SftpCredentials>();
	
	public string Login(SftpCredentials credentials)
	{
		var dev = Guid.NewGuid().ToString();
		_credentialsMap.Add(dev, credentials);
		return dev;
	}

	public void Logout(SftpCredentials credentials)
	{
		if (credentials.Token is not null && _credentialsMap.ContainsKey(credentials.Token))
		{
			_credentialsMap.Remove(credentials.Token);
		}
	}

	public SftpCredentials? Obtain(SftpCredentials token)
	{
		if (token.Token is not null && _credentialsMap.ContainsKey(token.Token))
		{
			return _credentialsMap[token.Token];
		}
		return null;
	}
}
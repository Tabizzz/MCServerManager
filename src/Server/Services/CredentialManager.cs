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
		credentials.IsValid = true;
		credentials.Token = dev;
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

	public SftpCredentials? Obtain(SftpCredentials credentials)
	{
		return credentials.Token is null || !_credentialsMap.TryGetValue(credentials.Token, out var value) ? null : value;
	}

	public SftpCredentials? GetSession(SftpCredentials credentials)
	{
		if (credentials.Token is null || !_credentialsMap.TryGetValue(credentials.Token, out var value))
			return null;
		var clone = (SftpCredentials)value.Clone();
		clone.Password = "";
		return clone;
	}
}
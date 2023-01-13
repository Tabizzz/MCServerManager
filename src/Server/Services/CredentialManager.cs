using System.Collections.Concurrent;
using WebServerManager.Shared;
namespace WebServerManager.Server.Services;

// todo: better auth method?
public class CredentialManager
{
	readonly IDictionary<string, SftpCredentials> _credentialsMap = new ConcurrentDictionary<string, SftpCredentials>();
	
	public SftpCredentials Login(SftpCredentials credentials)
	{
		var dev = Guid.NewGuid().ToString();
		credentials.IsValid = true;
		credentials.Token = dev;
		_credentialsMap.Add(dev, credentials);
		return credentials;
	}

	public bool Logout(SftpCredentials credentials)
	{
		if (credentials.Token is null || !_credentialsMap.ContainsKey(credentials.Token))
			return false;
		
		_credentialsMap.Remove(credentials.Token);
		return true;
	}

	public SftpCredentials? Obtain(SftpCredentials credentials) => 
		credentials.Token is not null ? Obtain(credentials.Token) : null;

	public SftpCredentials? Obtain(string token) => 
		_credentialsMap.TryGetValue(token, out var value) ? value : null;

	public SftpCredentials? GetSession(SftpCredentials credentials)
	{
		if (credentials.Token is null || !_credentialsMap.TryGetValue(credentials.Token, out var value))
			return null;
		var clone = (SftpCredentials)value.Clone();
		clone.Password = "";
		return clone;
	}
}
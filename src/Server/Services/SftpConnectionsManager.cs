using System.Collections.Concurrent;
using Renci.SshNet;
using WebServerManager.Server.Abstractions;
namespace WebServerManager.Server.Services;

public class SftpConnectionsManager : IAsyncRunnable
{

	readonly IDictionary<string, (DateTime lastUse, SftpClient connection)> _clientDictionary = 
		new ConcurrentDictionary<string, (DateTime, SftpClient)>();

	readonly CredentialManager _credentialManager;

	readonly ILogger<SftpConnectionsManager> _logger;

	public SftpConnectionsManager(CredentialManager credentialManager, ILogger<SftpConnectionsManager> logger)
	{
		_credentialManager = credentialManager;
		_logger = logger;
	}
	public SftpClient? CreateConnection(string key)
	{
		if (_credentialManager.Obtain(key) is not { Token: { } } credentials)
			return null;
		
		var client = new SftpClient(credentials.Host, credentials.Port, credentials.User, credentials.Password);
		client.Connect();
		
		_clientDictionary.Add(key, (DateTime.Now, client));
		return client;
	}

	public void DeleteConnection(string key)
	{
		if (!_clientDictionary.ContainsKey(key))
			return;

		_clientDictionary[key].connection.Dispose();
		_clientDictionary.Remove(key);
	}
	
	public TimeSpan MonitorInterval { get; } = TimeSpan.FromMinutes(1);
	
	public Task Run(CancellationToken _)
	{
		foreach (var key in _clientDictionary.Keys)
		{
			var client = _clientDictionary[key];
			var unusedTime = DateTime.Now - client.lastUse;
			if (unusedTime > MonitorInterval)
			{
				client.connection.Dispose();
				_clientDictionary.Remove(key);
			}
		}
		return Task.CompletedTask;
	}

	public ValueTask DisposeAsync()
	{
		foreach (var client in _clientDictionary)
		{
			client.Value.connection.Dispose();
		}
		return ValueTask.CompletedTask;
	}

	public SftpClient? GetConnection(string key)
	{
		if (!_clientDictionary.TryGetValue(key, out var value))
		{
			try
			{
				if (CreateConnection(key) is { } client)
				{
					value = (DateTime.Now, client);
				}
				else
				{
					return null;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on client creation");
				return null;
			}
		}

		if(!value.connection.IsConnected)
			value.connection.Connect();
		value.lastUse = DateTime.Now;
		return value.connection;
	}
}
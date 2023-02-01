using MCServerManager.Desktop.Models;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
namespace MCServerManager.Desktop.Managers;

/*
 * Manage all active connections to sftp servers
 */
public class SftpConnectionsManager : BaseManager<Guid, (DateTime lastUse, SftpClient connection)>
{

	readonly ServerManager _serverManager;

	readonly ILogger<SftpConnectionsManager> _logger;

	public SftpConnectionsManager(ServerManager serverManager, ILogger<SftpConnectionsManager> logger)
	{
		_serverManager = serverManager;
		_logger = logger;
	}
	
	public async Task<SftpClient?> CreateConnection(Guid key, bool force = false)
	{
		if (_serverManager[key] is not { Sftp: { } credentials })
			return null;
		if (_dictionary.TryGetValue(key, out var value))
		{
			if (force)
			{
				value.connection.Disconnect();
				_dictionary.Remove(key);
			}
			else
			{
				return value.connection;
			}
		}

		var client = new SftpClient(credentials.Host, credentials.Port, credentials.User, credentials.Password);
		await client.ConnectAsync(CancellationToken.None);
		
		_dictionary.Add(key, (DateTime.Now, client));
		return client;
	}
	
	internal async Task<SftpClient> CreateConnection(MCServer server)
	{
		if (_dictionary.TryGetValue(server.Id, out var value))
		{
			value.connection.Disconnect();
			_dictionary.Remove(server.Id);
		}

		var client = new SftpClient(server.Sftp.Host, server.Sftp.Port, server.Sftp.User, server.Sftp.Password);
		await client.ConnectAsync(CancellationToken.None);
		
		_dictionary.Add(server.Id, (DateTime.Now, client));
		return client;
	}

	public void DeleteConnection(Guid key)
	{
		if (!_dictionary.ContainsKey(key))
			return;

		_dictionary[key].connection.Dispose();
		_dictionary.Remove(key);
	}
	
	public TimeSpan MonitorInterval { get; } = TimeSpan.FromMinutes(1);
	

	public ValueTask DisposeAsync()
	{
		foreach (var client in _dictionary)
		{
			client.Value.connection.Dispose();
		}
		return ValueTask.CompletedTask;
	}

	public async Task<SftpClient?> GetConnection(Guid key)
	{
		if (!_dictionary.TryGetValue(key, out var value))
		{
			try
			{
				if (await CreateConnection(key) is { } client)
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
			await value.connection.ConnectAsync(CancellationToken.None);
		value.lastUse = DateTime.Now;
		return value.connection;
	}
}
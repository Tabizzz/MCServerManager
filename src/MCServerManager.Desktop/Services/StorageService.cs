using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Models;
using mcswlib.ServerStatus;
using Microsoft.JSInterop;
using Newtonsoft.Json;
namespace MCServerManager.Desktop.Services;

public class StorageService
{
	readonly ServerManager _serverManager;

	readonly SftpConnectionsManager _connectionsManager;

	readonly ServerStatusFactory _serverFactory;

	readonly List<MCServer> _invalidServers = new();

	MCServer[] InvalidServers => _invalidServers.ToArray();

	public bool HasInvalid => _invalidServers.Count > 0;

	public StorageService(ServerManager serverManager, SftpConnectionsManager connectionsManager, ServerStatusFactory serverFactory)
	{
		_serverManager = serverManager;
		_connectionsManager = connectionsManager;
		_serverFactory = serverFactory;
	}

	public bool Loaded { get; private set; }
	
	public async Task Save(IJSRuntime jsRuntime)
	{
		await jsRuntime.InvokeVoidAsync("localStorage.setItem","MCServers", JsonConvert.SerializeObject(_serverManager.Servers));
	}

	public async Task Load(IJSRuntime jsRuntime)
	{
		var content = await jsRuntime.InvokeAsync<string?>("localStorage.getItem","MCServers");
		if (content is not null && JsonConvert.DeserializeObject<MCServer[]>(content) is { } servers)
		{
			foreach (var server in servers)
			{
				if (_serverManager.Register(server))
				{
					await LoadServer(server);
				}
			}
		}
		_serverFactory.StartAutoUpdate(5);
		Loaded = true;
	}

	async Task LoadServer(MCServer server)
	{
		try
		{
			var conn = await _connectionsManager.CreateConnection(server.Id);
			if (conn is null)
				throw new("Something is wrong");
		}
		catch (Exception)
		{
			_serverManager.Remove(server.Id);
			_invalidServers.Add(server);
			return;
		}
		
		_serverFactory.Make(server.Ip, server.Port, true, server.Name);
	}
}
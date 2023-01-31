using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Models;
using MCServerManager.Desktop.Utils.Extensions;
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

	readonly TaskService _taskService;

	MCServer[] InvalidServers => _invalidServers.ToArray();

	public bool HasInvalid => _invalidServers.Count > 0;

	public StorageService(ServerManager serverManager, SftpConnectionsManager connectionsManager, ServerStatusFactory serverFactory, TaskService taskService)
	{
		_serverManager = serverManager;
		_connectionsManager = connectionsManager;
		_serverFactory = serverFactory;
		_taskService = taskService;
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
		MakeFirstPing();
		Loaded = true;
	}

	void MakeFirstPing()
	{
		_taskService.Create(new()
		{
			Tittle = "Pinging Servers",
			TaskCreator = FirstPingCore
		});
	}

	async Task FirstPingCore(RunningBackgroundTask arg)
	{
		var c = _serverManager.Count;
		float t = 0;
		foreach (var server in _serverManager.Servers)
		{
			arg.BackgroundTask.Description = $"Pinging {server.Name}";
			arg.BackgroundTask.Progress = t++ / c * 100;
			arg.Update();
			var pinger = _serverFactory.MakeOrGet(server.Ip, server.Port, server.Name);
			await pinger.Updater.Ping();
			var events = pinger.Update();
			_serverFactory.ServerChanged?.Invoke(pinger, events);
		}
		_serverFactory.StartAutoUpdate(5);
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
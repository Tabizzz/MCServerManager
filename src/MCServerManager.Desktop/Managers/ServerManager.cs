using MCServerManager.Desktop.Models;
using mcswlib.ServerStatus;
using MessagePipe;
namespace MCServerManager.Desktop.Managers;

/*
 * Manages all the servers the user add
 */
public class ServerManager : BaseManager<Guid, MCServer>, IDisposable
{
	readonly IAsyncPublisher<MCServer> _publisher;

	readonly IDisposable _disposable;

	/*
	 * The current selected server.
	 */
	public MCServer? CurrentServer { get; private set; }

	public IEnumerable<MCServer> Servers => _dictionary.Values;

	public ServerManager(IAsyncPublisher<MCServer> publisher, IAsyncSubscriber<ServerStatus> subscriber)
	{
		_disposable = DisposableBag.Create(subscriber.Subscribe(OnPing));
		_publisher = publisher;
	}

	async ValueTask OnPing(ServerStatus status, CancellationToken token)
	{
		var server = _dictionary.Values.First(t=>t.Ip == status.Updater.Address && t.Port == status.Updater.Port);
		server.Status = status.Updater.GetLatestServerInfo();
		await _publisher.PublishAsync(server, token);
	}

	public bool Register(MCServer server)
	{
		if (_dictionary.ContainsKey(server.Id))
			return false;
		_dictionary.Add(server.Id, server);
		if (Count == 1)
			CurrentServer = server;
		_publisher.PublishAsync(server);
		return true;
	}

	public void Remove(Guid id)
	{
		if (!_dictionary.ContainsKey(id))
			return;
		var server = _dictionary[id];
		_dictionary.Remove(id);
		if (Count == 0)
			CurrentServer = null;
		else if (CurrentServer == server)
			CurrentServer = _dictionary.First().Value;
		_publisher.PublishAsync(server);
	}
	
	public async Task<bool> Select(Guid id)
	{
		if (!_dictionary.TryGetValue(id, out var server))
			return false;
		
		CurrentServer = server;
		await _publisher.PublishAsync(CurrentServer, AsyncPublishStrategy.Parallel);
		return true;
	}
	public void Dispose()
	{
		_disposable.Dispose();
	}
}
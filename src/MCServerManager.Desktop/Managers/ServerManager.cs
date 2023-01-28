using MCServerManager.Desktop.Models;
using MessagePipe;
namespace MCServerManager.Desktop.Managers;

/*
 * Manages all the servers the user add
 */
public class ServerManager : BaseManager<Guid, MCServer>
{
	readonly IAsyncPublisher<MCServer> _publisher;
	
	/*
	 * The current selected server.
	 */
	public MCServer? CurrentServer { get; private set; }
	
	public ServerManager(IAsyncPublisher<MCServer> publisher)
	{
		_publisher = publisher;
	}
	
	public bool Register(MCServer server)
	{
		if (_dictionary.ContainsKey(server.Id))
			return false;
		_dictionary.Add(server.Id, server);
		return true;
	}

	public async Task<bool> Select(Guid id)
	{
		if (!_dictionary.TryGetValue(id, out var server))
			return false;
		
		CurrentServer = server;
		await _publisher.PublishAsync(CurrentServer, AsyncPublishStrategy.Parallel);
		return true;
	}
}
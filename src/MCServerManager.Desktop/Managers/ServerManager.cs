using MCServerManager.Desktop.Models;
namespace MCServerManager.Desktop.Managers;

/*
 * Manages all the servers the user add
 */
public class ServerManager
{
	/*
	 * The current server selected.
	 */
	public MCServer? CurrentServer { get; private set; }
	
	public uint ServerCount { get; private set; }
	
	
}
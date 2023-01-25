namespace MCServerManager.Desktop.Models;

// ReSharper disable once InconsistentNaming
/*
 * A remote server that store the credentials to allow different services to work.
 */
public class MCServer
{
	/*
	 * Create a new server with a specfic id.
	 */
	public MCServer(Guid id)
	{
		Id = id;
	}

	/*
	 * Unique id of this server.
	 */
	public Guid Id { get; private set; }
	
	/*
	 * Path for the icon to show in the server.
	 */
	public string Icon { get; set; }

	/*
	 * User provided display name of the server.
	 */
	public string Name { get; set; } = string.Empty;

	/*
	 * The ip of the server.
	 */
	public string Ip { get; set; } = string.Empty;
	
	/*
	 * Port of the server.
	 */
	public ushort Port { get; set; } = 25565;
	
	/*
	 * Sftp credentials to access to the remote file sistem.
	 */
	public SftpCredentials? Sftp { get; set; }
}
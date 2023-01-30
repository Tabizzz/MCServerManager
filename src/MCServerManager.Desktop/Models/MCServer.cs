using System.Text.Json.Serialization;
using mcswlib.ServerStatus.ServerInfo;
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
	public Guid Id { get; }
	
	/*
	 * Path for the icon to show in the server.
	 */
	public string Icon { get; set; } = string.Empty;

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
	 * Sftp credentials to access to the remote file system.
	 */
	public SftpCredentials Sftp { get; set; } = new();

	[Newtonsoft.Json.JsonIgnore]
	[JsonIgnore]
	/*
	 * Status of the server pings.
	 */
	public ServerInfoResult? Status { get; set; }
}
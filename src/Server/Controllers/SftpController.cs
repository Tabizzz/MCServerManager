using System.Net;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using WebServerManager.Server.Services;
using WebServerManager.Shared;
namespace WebServerManager.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SftpController : ControllerBase
{
	readonly ILogger<SftpController> _logger;

	readonly CredentialManager _credentialManager;

	public SftpController(ILogger<SftpController> logger, CredentialManager credentialManager)
	{
		_credentialManager = credentialManager;
		_logger = logger;
	}

	[HttpPost]
	public IEnumerable<SftpFileEntry> ListFiles([FromBody] SftpCredentials token, [FromQuery] string path)
	{
		_logger.LogInformation("Listing directory \"{Path}\" for {Token}", path, token.Token);
		var credentials = _credentialManager.Obtain(token);
		if (credentials is not null)
		{
			try
			{
				using var client = new SftpClient(credentials.Host, credentials.Port, credentials.User, credentials.Password );
				client.Connect();
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				var files = client.ListDirectory(path);
				return files.Select(f => new SftpFileEntry
				{
					Path = f.FullName,
					IsFolder = f.IsDirectory,
					Size = f.Length,
					Name = f.Name,
					LastWrite = f.LastWriteTime
				});
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}	
		}
		else
		{
			Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}

		return Array.Empty<SftpFileEntry>();
	}

	[HttpPost]
	public IActionResult ReadFile([FromBody] SftpCredentials token, [FromQuery] string path)
	{
		_logger.LogInformation("Reading file \"{Path}\" for {Token}", path, token.Token);
		var credentials = _credentialManager.Obtain(token);
		if (credentials is null)
			return Unauthorized();
		try
		{
			var client = new SftpClient(credentials.Host, credentials.Port, credentials.User, credentials.Password );
			client.Connect();
			Response.StatusCode = (int)HttpStatusCode.Accepted;
			var filed = client.Get(path);
			var file = client.OpenRead(path);
			Response.OnCompleted( async () =>
			{
				await file.DisposeAsync();
				client.Dispose();
			});
			return File(file, "application/octet-stream", filed.Name);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error on authentication");
			return Unauthorized();
		}
	}
}
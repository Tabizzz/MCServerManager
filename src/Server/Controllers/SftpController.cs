using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebServerManager.Server.Services;
using WebServerManager.Shared;
namespace WebServerManager.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SftpController : ControllerBase
{
	readonly ILogger<SftpController> _logger;

	readonly CredentialManager _credentialManager;

	readonly SftpConnectionsManager _sftpConnectionsManager;

	public SftpController(ILogger<SftpController> logger, CredentialManager credentialManager, SftpConnectionsManager sftpConnectionsManager)
	{
		_credentialManager = credentialManager;
		_sftpConnectionsManager = sftpConnectionsManager;
		_logger = logger;
	}

	[HttpPost]
	public IEnumerable<SftpFileEntry> ListFiles([FromBody] SftpCredentials token, [FromQuery] string path)
	{
		_logger.LogInformation("Listing directory \"{Path}\" for {Token}", path, token.Token);

		if (_credentialManager.Obtain(token) is { Token: { } } credentials)
		{
			try
			{
				if (_sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
				{
					var files = client.ListDirectory(path);
					Response.StatusCode = (int)HttpStatusCode.Accepted;
					return files.Select(f => new SftpFileEntry
					{
						Path = f.FullName,
						IsFolder = f.IsDirectory,
						Size = f.Length,
						Name = f.Name,
						LastWrite = f.LastWriteTime
					});
				}
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

	[HttpPost]
	public string RawText([FromBody] SftpCredentials token, [FromQuery] string path)
	{
		_logger.LogInformation("Reading text file \"{Path}\" for {Token}", path, token.Token);
		if (_credentialManager.Obtain(token) is { Token: { } } credentials)
		{
			try
			{
				if (_sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
				{
					var info = client.Get(path);
					using var text = client.OpenText(path);
					if (text is not null)
					{
						Response.StatusCode = (int)HttpStatusCode.Accepted;
						if (info.Length < 5e6)
							return text.ReadToEnd();
						Response.StatusCode = (int)HttpStatusCode.InsufficientStorage;
						return info.Length + ";" + 5e6;
					}
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}

		Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		return string.Empty;
	}

	[HttpPost]
	public IActionResult ReadFile([FromBody] SftpCredentials token, [FromQuery] string path)
	{
		_logger.LogInformation("Reading file \"{Path}\" for {Token}", path, token.Token);
		if (_credentialManager.Obtain(token) is not { Token: { } } credentials)
			return Unauthorized();
		
		try
		{
			if (_sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
			{
				var filed = client.Get(path);
				using var file = client.OpenRead(path);
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				return File(file, "application/octet-stream", filed.Name);
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error on authentication");
		}
		return Unauthorized();
	}
}
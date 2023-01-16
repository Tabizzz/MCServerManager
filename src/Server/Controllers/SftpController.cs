using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Sftp;
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
	public void RenameFile([FromBody] SftpCredentials token, [FromQuery] string src, [FromQuery] string dest)
	{
		_logger.LogInformation("Renamig file \"{Path}\" to \"{Dest}\" for {Token}", src , dest, token.Token);
		if(src.Equals("/")) return;
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				var exits = client.Exists(dest);
				if (exits)
				{
					Response.StatusCode = (int)HttpStatusCode.NoContent;
					return;
				}
				client.RenameFile(src, dest);
				
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		Response.StatusCode = (int)HttpStatusCode.Unauthorized;
	}
	
	[HttpPost]
	public void DeleteFile([FromBody] SftpCredentials token, [FromQuery] string path)
	{
		_logger.LogInformation("Deleting file \"{Path}\" for {Token}", path , token.Token);
		if(path.Equals("/")) return;
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				client.Delete(path);
				
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		Response.StatusCode = (int)HttpStatusCode.Unauthorized;
	}
	
	[DisableRequestSizeLimit]
	[HttpPost]
	public void UploadFile([FromForm]IFormFile file, [FromQuery] string path, [FromQuery] string tokenstr)
	{
		_logger.LogInformation("Uploading file \"{Path}\" for {Token}", path , tokenstr);
		var token = new SftpCredentials { Token = tokenstr };

		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				using (var fileStream = file.OpenReadStream())
				{
					client.UploadFile(fileStream, path, true, _ => { });
				}
				
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		Response.StatusCode = (int)HttpStatusCode.Unauthorized;
	}

	[HttpPost]
	public void CreateEmptyFile([FromBody] SftpCredentials token, [FromQuery] string path, [FromQuery] bool directory)
	{
		_logger.LogInformation("Creating empty {Type} \"{Path}\" for {Token}", directory?"directory" : "file", path , token.Token);
		if (_credentialManager.Obtain(token) is { Token: { } } credentials && 
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				var exits = client.Exists(path);
				if (exits)
				{
					Response.StatusCode = (int)HttpStatusCode.NoContent;
					return;
				}
				
				if(directory)
					client.CreateDirectory(path);
				else
					client.Create(path).Dispose();
				
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		
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
	public void UpdateRawText(UpdateTextFileRequest request)
	{
		_logger.LogInformation("Updating text file \"{Path}\" for {Token}", request.Path, request.Credentials.Token);
		if (_credentialManager.Obtain(request.Credentials) is { Token: { } } credentials && 
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				client.WriteAllText(request.Path, request.Content);
				Response.StatusCode = (int)HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		Response.StatusCode = (int)HttpStatusCode.Unauthorized;
	}

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
					using var text = client.OpenText(path)!;
					Response.StatusCode = (int)HttpStatusCode.Accepted;
					if (info.Length < 2e6)
						return text.ReadToEnd();
					Response.StatusCode = (int)HttpStatusCode.InsufficientStorage;
					return info.Length + ";" + 5e6;
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

	[HttpGet]
	public IActionResult ReadFile([FromQuery] string tokenstr, [FromQuery] string path)
	{
		var token = new SftpCredentials { Token = tokenstr };
		_logger.LogInformation("Reading file \"{Path}\" for {Token}", path, token.Token);
		if (_credentialManager.Obtain(token) is not { Token: { } } credentials)
			return Unauthorized();
		
		try
		{
			if (_sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
			{
				var filed = client.Get(path);
				var file = client.OpenRead(path);
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
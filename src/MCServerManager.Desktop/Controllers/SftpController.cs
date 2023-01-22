using System.Net;
using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Models;
using MCServerManager.Desktop.Utils;
using MCServerManager.Desktop.Utils.Extensions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
namespace MCServerManager.Desktop.Controllers;

public class SftpController
{
	readonly ILogger<SftpController> _logger;

	readonly CredentialManager _credentialManager;

	readonly SftpConnectionsManager _sftpConnectionsManager;
	
	public HttpStatusCode StatusCode { get; private set; }

	public SftpController(ILogger<SftpController> logger, CredentialManager credentialManager, SftpConnectionsManager sftpConnectionsManager)
	{
		_credentialManager = credentialManager;
		_sftpConnectionsManager = sftpConnectionsManager;
		_logger = logger;
	}

	public Task RenameFile(SftpCredentials token, string src, string dest) => Task.Run(() =>
	{
		_logger.LogInformation("Renamig file \"{Path}\" to \"{Dest}\" for {Token}", src, dest, token.Token);
		if (src.Equals("/")) return;
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				var exits = client.Exists(dest);
				if (exits)
				{
					StatusCode = HttpStatusCode.NoContent;
					return;
				}
				client.RenameFile(src, dest);

				StatusCode = HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}

		StatusCode = HttpStatusCode.Unauthorized;
	});

	public Task DeleteFile(SftpCredentials token, string path) => Task.Run(() =>
	{
		_logger.LogInformation("Deleting file \"{Path}\" for {Token}", path , token.Token);
		if(path.Equals("/")) return;
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				client.Delete(path);
				
				StatusCode = HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		StatusCode = HttpStatusCode.Unauthorized;
	});

	
	public async Task UploadFile(IBrowserFile file, string path, SftpCredentials token, Action<ulong> action)
	{
		_logger.LogInformation("Uploading file \"{Path}\" for {Token}", path , token.Token);

		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				await using (var fileStream = file.OpenReadStream(long.MaxValue))
				{
					var input = new StreamAdapter(fileStream);

					await Task.Factory.FromAsync(
						(a,b, c, d, e)=> client.BeginUploadFile(a, b, true, d, e, c), 
						client.EndUploadFile, 
						input, 
						path, 
						action, 
						null);
				}
				StatusCode = HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		StatusCode = HttpStatusCode.Unauthorized;
	}

	public Task CreateEmptyFile(SftpCredentials token, string path, bool directory) => Task.Run(() =>
	{
		_logger.LogInformation("Creating empty {Type} \"{Path}\" for {Token}", directory ? "directory" : "file", path, token.Token);
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				var exits = client.Exists(path);
				if (exits)
				{
					StatusCode = HttpStatusCode.NoContent;
					return;
				}

				if (directory)
					client.CreateDirectory(path);
				else
					client.Create(path).Dispose();

				StatusCode = HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}

		StatusCode = HttpStatusCode.Unauthorized;

	});

	public async Task<IEnumerable<SftpFileEntry>> ListFiles( SftpCredentials token,  string path)
	{
		_logger.LogInformation("Listing directory \"{Path}\" for {Token}", path, token.Token);

		if (_credentialManager.Obtain(token) is { Token: { } } credentials)
		{
			try
			{
				if (_sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
				{
					var files = await client.ListDirectoryAsync(path);

					StatusCode = HttpStatusCode.Accepted;
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
				StatusCode = HttpStatusCode.Unauthorized;
			}
		}
		else
		{
			StatusCode = HttpStatusCode.Unauthorized;
		}

		return Array.Empty<SftpFileEntry>();
	}

	public Task UpdateRawText(string path, SftpCredentials token, string content) => Task.Run(() =>
	{
		_logger.LogInformation("Updating text file \"{Path}\" for {Token}", path, token.Token);
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				client.WriteAllText(path, content);
				StatusCode = HttpStatusCode.Accepted;
				return;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		StatusCode = HttpStatusCode.Unauthorized;
	});

	public Task<string> RawText(SftpCredentials token, string path) => Task.Run(() =>
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
					StatusCode = HttpStatusCode.Accepted;
					if (info.Length < 2e6)
						return text.ReadToEnd();
					StatusCode = HttpStatusCode.InsufficientStorage;
					return info.Length + ";" + 5e6;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}

		StatusCode = HttpStatusCode.Unauthorized;
		return string.Empty;
	});
	
	public async Task SaveFile(SftpCredentials token, string path, string localFile, Action<ulong> action)
	{
		_logger.LogInformation("Reading file \"{Path}\" for {Token}", path, token.Token);
		
		if (_credentialManager.Obtain(token) is { Token: { } } credentials &&
		    _sftpConnectionsManager.GetConnection(credentials.Token) is { } client)
		{
			try
			{
				await using var local = File.Exists(localFile) ? File.Open(localFile, FileMode.Truncate, FileAccess.Write) : File.OpenWrite(localFile);
				await Task.Factory.FromAsync(
					(a,b,c,d, e)=>client.BeginDownloadFile(a, b, d, e, c), 
					client.EndDownloadFile,
					path, local, action, null);
				StatusCode = HttpStatusCode.Accepted;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		StatusCode = HttpStatusCode.Unauthorized;
	}
}
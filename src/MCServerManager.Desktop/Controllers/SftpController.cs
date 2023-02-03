using System.Net;
using System.Text;
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

	readonly ServerManager _serverManager;

	readonly SftpConnectionsManager _sftpConnectionsManager;
	
	public HttpStatusCode StatusCode { get; private set; }

	public SftpController(ILogger<SftpController> logger, ServerManager serverManager, SftpConnectionsManager sftpConnectionsManager)
	{
		_serverManager = serverManager;
		_sftpConnectionsManager = sftpConnectionsManager;
		_logger = logger;
	}

	public async Task RenameFile(Guid id, string src, string dest)
	{
		_logger.LogInformation("Renamig file \"{Path}\" to \"{Dest}\" for {Token}", src, dest, id);
		if (src.Equals("/")) return;
		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				await Task.Run(() =>
				{
					var exits = client.Exists(dest);
					if (exits)
					{
						StatusCode = HttpStatusCode.NoContent;
						return;
					}
					client.RenameFile(src, dest);
				});

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

	public async Task DeleteFile(Guid id, string path)
	{
		_logger.LogInformation("Deleting file \"{Path}\" for {Token}", path , id);
		if(path.Equals("/")) return;
		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				await TaskUtils.Run(client.Delete, path);
				
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

	
	public async Task UploadFile(IBrowserFile file, string path, Guid id, Action<ulong> action)
	{
		_logger.LogInformation("Uploading file \"{Path}\" for {Token}", path , id);

		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				await using var fileStream = file.OpenReadStream(long.MaxValue);
				var input = new StreamAdapter(fileStream);

				await Task.Factory.FromAsync(
					(a,b, c, d, e)=> client.BeginUploadFile(a, b, true, d, e, c), 
					client.EndUploadFile, 
					input, 
					path, 
					action, 
					null);
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

	public async Task CreateEmptyFile(Guid id, string path, bool directory)
	{
		_logger.LogInformation("Creating empty {Type} \"{Path}\" for {Token}", directory ? "directory" : "file", path, id);
		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				var exits = await TaskUtils.Run(client.Exists, path);
				if (exits)
				{
					StatusCode = HttpStatusCode.NoContent;
					return;
				}

				if (directory)
					client.CreateDirectory(path);
				else
					await client.Create(path).DisposeAsync();

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

	public async Task<IEnumerable<SftpFileEntry>> ListFiles(Guid id,  string path)
	{
		_logger.LogInformation("Listing directory \"{Path}\" for {Token}", path, id);

		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				var files = await client.ListDirectoryAsync(path);

				StatusCode = HttpStatusCode.Accepted;
				return files.Where(t=>t.Name is not "." and not "..").Select(f => new SftpFileEntry
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
				StatusCode = HttpStatusCode.Unauthorized;
			}
		}
		else
		{
			StatusCode = HttpStatusCode.Unauthorized;
		}

		return Array.Empty<SftpFileEntry>();
	}

	public async Task UpdateRawText(string path, Guid id, string content)
	{
		_logger.LogInformation("Updating text file \"{Path}\" for {Token}", path, id);
		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				await using var writer = new StreamWriter(client.OpenWrite(path), Encoding.UTF8);
				await writer.WriteAsync(content);
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

	public async Task<string> RawText(Guid id, string path)
	{
		_logger.LogInformation("Reading text file \"{Path}\" for {Token}", path, id);
		if (_serverManager[id] is { Sftp: { } } &&
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				var info = client.Get(path);
				using var text = client.OpenText(path);
				StatusCode = HttpStatusCode.Accepted;
				if (info.Length < 2e6)
					return await text.ReadToEndAsync();
				StatusCode = HttpStatusCode.InsufficientStorage;
				return info.Length + ";" + 5e6;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}

		StatusCode = HttpStatusCode.Unauthorized;
		return string.Empty;
	}
	
	public async Task<HttpStatusCode> SaveFile(Guid id, string path, string localFile, Action<ulong> action)
	{
		_logger.LogInformation("Reading file \"{Path}\" for {Id}", path, id);
		
		if (_serverManager[id] is { Sftp: { } } && 
		    await _sftpConnectionsManager.GetConnection(id) is { } client)
		{
			try
			{
				await using var local = File.Exists(localFile) ? File.Open(localFile, FileMode.Truncate, FileAccess.Write) : File.OpenWrite(localFile);
				await Task.Factory.FromAsync(
					(a,b,c,d, e)=>client.BeginDownloadFile(a, b, d, e, c), 
					client.EndDownloadFile,
					path, local, action, null);
				return HttpStatusCode.Accepted;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
			}
		}
		return HttpStatusCode.Unauthorized;
	}
}
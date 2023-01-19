using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Web;
using MCServerManager.Desktop.Controllers;
using MCServerManager.Desktop.Models;
using MCServerManager.Desktop.Models.Enums;
namespace MCServerManager.Desktop.Services;

public class ServerPropertiesService
{
	readonly CredentialService _credentialService;

	readonly SftpController _sftp;

	public ServerProperties? Properties { get; private set; }

	int _estimatedSize;

	public ServerPropertiesService(CredentialService credentialService, SftpController sftp)
	{
		_credentialService = credentialService;
		_sftp = sftp;
		Properties = null;
	}
	
	public async Task ReadProperties()
	{
		if (Properties is null)
			await Reload();
	}

	async Task Reload()
	{
		var response = await _sftp.RawText(_credentialService.SftpCredentials, "/server.properties");
		if (_sftp.StatusCode == HttpStatusCode.Accepted)
		{
			_estimatedSize = response.Length;
			ParseProperties(response);
		}
	}

	void ParseProperties(string str)
	{
		var props = new ServerProperties();
		var type = typeof(ServerProperties);
		var sets = type.GetProperties();
		using (var sr = new StringReader(str)) {
			while (sr.ReadLine() is { } line)
			{
				if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
					continue;
				var ie = line.IndexOf("=", StringComparison.InvariantCulture);
				if(ie == -1) continue;
				var key = line[..ie];
				var value = line[(ie + 1)..];

				if (sets.FirstOrDefault(
					    s =>
					    {
						    if (s.GetCustomAttribute<DisplayNameAttribute>() is { } display)
						    {
							    return display.DisplayName == key;
						    }
						    return false;
					    }
				    ) is not { } set)
					continue;
				
				if (set.PropertyType == typeof(string))
				{
					set.SetMethod?.Invoke(props, new object?[] { value });
				}
				else if (set.PropertyType == typeof(bool?))
				{
					if (bool.TryParse(value, out var parsed))
					{
						set.SetMethod?.Invoke(props, new object?[] { parsed });
					}
				}
				else if (set.PropertyType == typeof(int?))
				{
					if (int.TryParse(value, out var parsed))
					{
						set.SetMethod?.Invoke(props, new object?[] { parsed });
					}
				}
				else if (set.PropertyType == typeof(long?))
				{
					if (long.TryParse(value, out var parsed))
					{
						set.SetMethod?.Invoke(props, new object?[] { parsed });
					}
				}
				else if(set.PropertyType == typeof(ServerGamemode?))
				{
					if (Enum.TryParse<ServerGamemode>(value, true, out var parsed))
					{
						set.SetMethod?.Invoke(props, new object?[] { parsed });
					}
					else if (int.TryParse(value, out var valparsed) && valparsed is >= 0 and < 4)
					{
						set.SetMethod?.Invoke(props, new object?[] { (ServerGamemode)valparsed });
					}
				}
				else if(set.PropertyType == typeof(ServerDifficulty?))
				{
					if (Enum.TryParse<ServerDifficulty>(value, true, out var parsed))
					{
						set.SetMethod?.Invoke(props, new object?[] { parsed });
					}
					else if (int.TryParse(value, out var valparsed) && valparsed is >= 0 and < 4)
					{
						set.SetMethod?.Invoke(props, new object?[] { (ServerDifficulty)valparsed });
					}
				}
			}
		}
		Properties = props;
	}

	public IEnumerable<PropertyInfo> Present()
	{
		var type = typeof(ServerProperties);
		return type.GetProperties().Where(p => p.GetMethod?.Invoke(Properties, null) is not null);
	}

	public string WriteProperties()
	{
		var builder = new StringBuilder(_estimatedSize);
		foreach (var info in Present())
		{
			var value = info.GetMethod?.Invoke(Properties, null)!.ToString()!;
			if (info.PropertyType != typeof(string))
			{
				value = value.ToLower();
			}
			builder.AppendLine(info.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName + "=" + value);
		}
		return builder.ToString();
	}
}
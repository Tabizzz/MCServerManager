using System.Net;
using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Models;
using Microsoft.Extensions.Logging;
namespace MCServerManager.Desktop.Controllers;

public class AuthController
{
	readonly ILogger<AuthController> _logger;

	readonly CredentialManager _credentialManager;

	readonly SftpConnectionsManager _sftpConnectionsManager;
	
	public HttpStatusCode StatusCode { get; private set; }

	public AuthController(ILogger<AuthController> logger, CredentialManager credentialManager, SftpConnectionsManager sftpConnectionsManager)
	{
		_credentialManager = credentialManager;
		_sftpConnectionsManager = sftpConnectionsManager;
		_logger = logger;
	}

	public SftpCredentials? GetSession(SftpCredentials credentials)
	{
		_logger.LogInformation("Restoring session {Token}", credentials.Token);
		var dev =  _credentialManager.GetSession(credentials);
		StatusCode = dev == null ? HttpStatusCode.Unauthorized : HttpStatusCode.Accepted;
		return dev;
	}

	public void LogOut(SftpCredentials credentials)
	{
		_logger.LogInformation("Logout user {Token}", credentials.Token);
		if(_credentialManager.Logout(credentials))
			_sftpConnectionsManager.DeleteConnection(credentials.Token!);
	}

	public Task<SftpCredentials> Login(SftpCredentials credentials)
	{
		return Task.Run<SftpCredentials>(()=>
		{
			_logger.LogInformation("Login user {Token}", credentials.User);
			try
			{
				var tosave = _credentialManager.Login(credentials);
				_sftpConnectionsManager.CreateConnection(tosave.Token!);
				StatusCode = HttpStatusCode.Accepted;
				return new()
				{
					Token = tosave.Token
				};
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error on authentication");
				StatusCode = HttpStatusCode.Unauthorized;
				return new()
				{
					Token = e.Message == "username" ? "Invalid username" : e.Message
				};
			}
		});
	}
}
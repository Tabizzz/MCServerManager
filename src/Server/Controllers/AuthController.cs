using System.Net;
using MCServerManager.Server.Services;
using MCServerManager.Shared;
using Microsoft.AspNetCore.Mvc;
namespace MCServerManager.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
	readonly ILogger<AuthController> _logger;

	readonly CredentialManager _credentialManager;

	readonly SftpConnectionsManager _sftpConnectionsManager;

	public AuthController(ILogger<AuthController> logger, CredentialManager credentialManager, SftpConnectionsManager sftpConnectionsManager)
	{
		_credentialManager = credentialManager;
		_sftpConnectionsManager = sftpConnectionsManager;
		_logger = logger;
	}

	[HttpPost]
	public SftpCredentials? GetSession(SftpCredentials credentials)
	{
		_logger.LogInformation("Restoring session {Token}", credentials.Token);
		var dev =  _credentialManager.GetSession(credentials);
		Response.StatusCode = (int)(dev == null ? HttpStatusCode.Unauthorized : HttpStatusCode.Accepted);
		return dev;
	}

	[HttpPost]
	public void LogOut(SftpCredentials credentials)
	{
		_logger.LogInformation("Logout user {Token}", credentials.Token);
		if(_credentialManager.Logout(credentials))
			_sftpConnectionsManager.DeleteConnection(credentials.Token!);
	}

	[HttpPost]
	public SftpCredentials Login(SftpCredentials credentials)
	{
		_logger.LogInformation("Login user {Token}", credentials.User);
		try
		{
			var tosave = _credentialManager.Login(credentials);
			_sftpConnectionsManager.CreateConnection(tosave.Token!);
			Response.StatusCode = (int)HttpStatusCode.Accepted;
			return new ()
			{
				Token = tosave.Token
			};
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Error on authentication");
			Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			return new ()
			{
				Token = e.Message == "username" ? "Invalid username" : e.Message
			};
		}
	}
}
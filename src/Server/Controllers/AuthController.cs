using System.Net;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using WebServerManager.Server.Services;
using WebServerManager.Shared;
namespace WebServerManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
	readonly ILogger<AuthController> _logger;

	readonly CredentialManager _credentialManager;

	public AuthController(ILogger<AuthController> logger, CredentialManager credentialManager)
	{
		_credentialManager = credentialManager;
		_logger = logger;
	}

	[HttpPost]
	[Route("session")]
	public SftpCredentials? GetSession(SftpCredentials credentials)
	{
		_logger.LogInformation("Restoring session {Token}", credentials.Token);
		var dev =  _credentialManager.GetSession(credentials);
		Response.StatusCode = (int)(dev == null ? HttpStatusCode.Unauthorized : HttpStatusCode.Accepted);
		return dev;
	}

	[HttpPost]
	[Route("logout")]
	public void LogOut(SftpCredentials credentials)
	{
		_logger.LogInformation("Logout user {Token}", credentials.Token);
		_credentialManager.Logout(credentials);
	}

	[HttpPost]
	public SftpCredentials Auth(SftpCredentials credentials)
	{
		_logger.LogInformation("Login user {Token}", credentials.User);
		try
		{
			using var client = new SftpClient(credentials.Host, credentials.Port, credentials.User, credentials.Password);
			client.Connect();
			Response.StatusCode = (int)HttpStatusCode.Accepted;
			return new ()
			{
				Token =  _credentialManager.Login(credentials)
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
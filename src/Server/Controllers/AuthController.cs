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
	public SftpCredentials? Auth(SftpCredentials credentials)
	{
		try
		{
			using var client = new SftpClient(credentials.Host, credentials.Port, credentials.User, credentials.Password );
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
				Token = e.Message
			};
		}
	}
}
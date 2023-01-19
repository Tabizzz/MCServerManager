using System.Net;
using MCServerManager.Desktop.Services;
using Microsoft.AspNetCore.Components;
namespace MCServerManager.Desktop.Pages;

public partial class Index
{
	[Inject] public CredentialService CredentialService { get; set; } = null!;
	
	public String ErrorMessage { get; set; } = "";
	
	public bool AuthLoading { get; set; }

	public async Task Logout()
	{
		Auth.LogOut(CredentialService.SftpCredentials);
		CredentialService.SftpCredentials.Token = null;
		await CredentialService.Clear();
		CredentialService.SftpCredentials.SaveCredentials = false;
		CredentialService.SftpCredentials.IsValid = false;
		await SftpPublisher.PublishAsync(CredentialService.SftpCredentials);
	}

	public async Task OnClickConnect()
	{
		ErrorMessage = "";
		AuthLoading = true;
		StateHasChanged();
		
		var token = await Auth.Login(CredentialService.SftpCredentials);

		switch (Auth.StatusCode)
		{
			case HttpStatusCode.Accepted:
				CredentialService.SftpCredentials.Token = token.Token;
				await CredentialService.Save();
				CredentialService.SftpCredentials.Password = string.Empty;
				CredentialService.SftpCredentials.IsValid = true;
				break;
			case HttpStatusCode.Unauthorized:
				ErrorMessage = token.Token!;
				break;
			default:
				ErrorMessage = "An unknow error hass ocurred";
				break;
		}

		await SftpPublisher.PublishAsync(CredentialService.SftpCredentials);
		AuthLoading = false;
		StateHasChanged();
	}
}

using System.Net;
using System.Net.Http.Json;
using MCServerManager.Client.Services;
using MCServerManager.Shared;
using Microsoft.AspNetCore.Components;
namespace MCServerManager.Client.Pages;

public partial class Index
{
	[Inject] public CredentialService CredentialService { get; set; } = null!;
	
	public String ErrorMessage { get; set; } = "";
	
	public bool AuthLoading { get; set; }

	public async Task Logout()
	{
		await Http.PostAsJsonAsync("Auth/logout", CredentialService.SftpCredentials);
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
		var response = await Http.PostAsJsonAsync("Auth/Login", CredentialService.SftpCredentials);
		AuthLoading = false;
		var token = await response.Content.ReadFromJsonAsync<SftpCredentials>();
		switch (response.StatusCode)
		{
			case HttpStatusCode.Accepted when token != null:
				CredentialService.SftpCredentials.Token = token.Token;
				await CredentialService.Save();
				CredentialService.SftpCredentials.Password = string.Empty;
				CredentialService.SftpCredentials.IsValid = true;
				break;
			case HttpStatusCode.Unauthorized when token != null:
				ErrorMessage = token.Token!;
				break;
			default:
				ErrorMessage = "An unknow error hass ocurred";
				break;
		}
		await SftpPublisher.PublishAsync(CredentialService.SftpCredentials);
		StateHasChanged();
	}
}

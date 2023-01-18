using MCServerManager.Server.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<CredentialManager>();
builder.Services.AddSingleton<SftpConnectionsManager>();
builder.Services.AddHostedService<MonitoringService<SftpConnectionsManager>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapFallbackToFile("/files/{*path}", "index.html");
app.MapFallbackToFile("/raw/{*path}", "index.html");

app.Run();
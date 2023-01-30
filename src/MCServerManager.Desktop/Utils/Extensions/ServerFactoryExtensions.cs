using mcswlib.ServerStatus;
namespace MCServerManager.Desktop.Utils.Extensions;

public static class ServerFactoryExtensions
{
	public static ServerStatus MakeOrGet(this ServerStatusFactory factory, string ip, ushort port, string label)
	{
		var server = factory.Entries.FirstOrDefault(s=>s.Updater.Address == ip && s.Updater.Port == port);
		return server ?? factory.Make(ip, port, true, label);
	}
}
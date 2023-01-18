using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCServerManager.Client.Models.Enums;
namespace MCServerManager.Client.Models;

public class ServerProperties
{
	[Description("""
Allows users to use flight on the server while in Survival mode.
With allow-flight enabled, griefers may become more common, because it makes their work easier. In Creative mode, this has no effect.

  <b>false</b> - Flight is not allowed (players in air for at least 5 seconds get kicked).
  <b>true</b> - Flight is allowed, and used if the player has a fly mod installed.
""")]
	[DisplayName("allow-flight")]
	[DefaultValue(false)]
	public bool? AllowFlight { get; set; }

	[Description("""
Allows players to travel to the Nether.

  <b>false</b> - Nether portals do not work.
  <b>true</b> - The server allows portals to send players to the Nether.
""")]
	[DisplayName("allow-nether")]
	[DefaultValue(true)]
	public bool? AllowNether { get; set; }

	[Description("Send console command outputs to all online operators.")]
	[DisplayName("broadcast-console-to-ops")]
	[DefaultValue(true)]
	public bool? BroadcastConsoleToOps { get; set; }

	[Description("Send rcon console command outputs to all online operators.")]
	[DisplayName("broadcast-rcon-to-ops")]
	[DefaultValue(true)]
	public bool? BroadcastRconToOps { get; set; }

	[Description("Defines the difficulty (such as damage dealt by mobs and the way hunger and poison affects players) of the server.")]
	[DisplayName("difficulty")]
	[DefaultValue(ServerDifficulty.Easy)]
	public ServerDifficulty? Difficulty { get; set; }

	[Description("Enables command blocks")]
	[DisplayName("enable-command-block")]
	[DefaultValue(false)]
	public bool? EnableCommandBlock { get; set; }

	[Description("""
Exposes an MBean with the Object name <code>net.minecraft.server:type=Server</code> and two attributes <code>averageTickTime</code> and <code>tickTimes</code> exposing the tick times in milliseconds.

In order for enabling JMX on the Java runtime you also need to add a couple of JVM flags to the startup as documented <a target="_blank" rel="nofollow" class="external text" href="https://docs.oracle.com/javase/8/docs/technotes/guides/management/agent.html">here</a>.
""")]
	[DisplayName("enable-jmx-monitoring")]
	[DefaultValue(false)]
	public bool? EnableJmxMonitoring { get; set; }

	[Description("""
Enables remote access to the server console.
   It's not recommended to expose RCON to the Internet, because RCON protocol transfers everything without encryption. Everything (including RCON password) communicated between the RCON server and client can be leaked to someone listening in on your connection.
""")]
	[DisplayName("enable-rcon")]
	[DefaultValue(false)]
	public bool? EnableRcon { get; set; }

	[Description("""
Makes the server appear as "online" on the server list.
If set to false, it will suppress replies from clients. This means it will appear as offline, but will still accept connections.
""")]
	[DisplayName("enable-status")]
	[DefaultValue(true)]
	public bool? EnableStatus { get; set; }

	[Description("Enables GameSpy4 protocol server listener. Used to get information about server.")]
	[DisplayName("enable-query")]
	[DefaultValue(false)]
	public bool? EnableQuery { get; set; }

	[Description("If set to <b>true</b>, players without a Mojang-signed public key will not be able to connect to the server.")]
	[DisplayName("enforce-secure-profile")]
	[DefaultValue(true)]
	public bool? EnforceSecureProfile { get; set; }

	[Description("""
Enforces the whitelist on the server.
When this option is enabled, users who are not present on the whitelist (if it's enabled) get kicked from the server after the server reloads the whitelist file.

  <b>false</b> - No user gets kicked if not on the whitelist.
  <b>true</b> - Online users not on the whitelist get kicked.
""")]
	[DisplayName("enforce-whitelist")]
	[DefaultValue(false)]
	public bool? EnforceWhitelist { get; set; }

	[Description("Controls how close entities need to be before being sent to clients. Higher values means they'll be rendered from farther away, potentially causing more lag. This is expressed the percentage of the default value. For example, setting to 50 will make it half as usual. This mimics the function on the client video settings (not unlike Render Distance, which the client can customize so long as it's under the server's setting).")]
	[DisplayName("entity-broadcast-range-percentage")]
	[DefaultValue(100)]
	[Range(10, 1000)]
	public int? EntityBroadcastRangePercentage { get; set; }

	[Description("""
Force players to join in the default game mode.

  <b>false</b> - Players join in the gamemode they left in.
  <b>true</b> - Players always join in the default gamemode.
""")]
	[DisplayName("force-gamemode")]
	[DefaultValue(false)]
	public bool? ForceGamemode { get; set; }

	[Description("""
Sets the default permission level for <a href="https://minecraft.fandom.com/wiki/Function_(Java_Edition)" title="Function (Java Edition)" target="_blank">functions</a>.
See <a href="https://minecraft.fandom.com/wiki/Permission_level" title="Permission level" target="_blank">permission level</a> level for the details on the 4 levels.
""")]
	[DisplayName("function-permission-level")]
	[DefaultValue(2)]
	[Range(1, 4)]
	public int? FunctionPermissionLevel { get; set; }

	[Description("Defines the mode of gameplay.")]
	[DisplayName("gamemode")]
	[DefaultValue(ServerGamemode.Survival)]
	public ServerGamemode? Gamemode { get; set; }

	[Description("""
Defines whether structures (such as villages) can be generated.

  <b>false</b> - Structures are not generated in new chunks.
  <b>true</b> - Structures are generated in new chunks.

<b>Note:</b> Dungeons still generate if this is set to false.
""")]
	[DisplayName("generate-structures")]
	[DefaultValue(true)]
	public bool? GenerateStructures { get; set; }

	[Description("""The settings used to customize world generation. Follow <a href="https://minecraft.fandom.com/wiki/Java_Edition_level_format#generatorOptions_tag_format" title="Java Edition level format" target="_blank">its format</a> and write the corresponding JSON string. Remember to escape all <code>:</code> with <code>\:</code>.""")]
	[DisplayName("generator-settings")]
	[DefaultValue("{}")]
	public string? GeneratorSettings { get; set; }

	[Description("If set to <b>true</b>, server difficulty is ignored and set to hard and players are set to spectator mode if they die.")]
	[DisplayName("hardcore")]
	[DefaultValue(false)]
	public bool? Hardcore { get; set; }

	[Description("If set to <b>true</b>, a player list is not sent on status requests.")]
	[DisplayName("hide-online-players")]
	[DefaultValue(false)]
	public bool? HideOnlinePlayers { get; set; }

	[Description("Comma-separated list of datapacks to not be auto-enabled on world creation.")]
	[DisplayName("initial-disabled-packs")]
	[DefaultValue("")]
	public string? InitialDisabledPacks { get; set; }

	[Description("Comma-separated list of datapacks to be enabled during world creation. Feature packs need to be explicitly enabled.")]
	[DisplayName("initial-enabled-packs")]
	[DefaultValue("vanilla")]
	public string? InitialEnabledPacks { get; set; }

	[Description("""
The "level-name" value is used as the world name and its folder name. The player may also copy their saved game folder here, and change the name to the same as that folder's to load it instead.
Characters such as ' (apostrophe) may need to be escaped by adding a backslash before them.
""")]
	[DisplayName("level-name")]
	[DefaultValue("world")]
	public string? LevelName { get; set; }

	[Description("Sets a world seed for the player's world, as in Singleplayer. The world generates with a random seed if left blank.")]
	[DisplayName("level-seed")]
	[DefaultValue("")]
	public string? LevelSeed { get; set; }

	[Description("""
Determines the world preset that is generated.
Escaping ":" is required when using a world preset ID, and the vanilla world preset ID's namespace (<code>minecraft:</code>) can be omitted.

See <a href="https://minecraft.fandom.com/wiki/Server.properties" target="_blank">the wiki</a> for more info.
""")]
	[DisplayName("level-type")]
	[DefaultValue("normal")]
	public string? LevelType { get; set; }

	[Description("Limiting the amount of consecutive neighbor updates before skipping additional ones. Negative values remove the limit.")]
	[DisplayName("max-chained-neighbor-updates")]
	[DefaultValue(1000000)]
	public int? MaxChainedNeighborUpdates { get; set; }

	[Description("The maximum number of players that can play on the server at the same time. Note that more players on the server consume more resources. Note also, op player connections are not supposed to count against the max players, but ops currently cannot join a full server. However, this can be changed by going to the file called ops.json in the player's server directory, opening it, finding the op that the player wants to change, and changing the setting called bypassesPlayerLimit to true (the default is false). This means that that op does not have to wait for a player to leave in order to join. Extremely large values for this field result in the client-side user list being broken.")]
	[DisplayName("max-players")]
	[DefaultValue(20)]
	[Range(0, 2147483647)]
	public int? MaxPlayers { get; set; }

	[Description("""
The maximum number of milliseconds a single tick may take before the server watchdog stops the server with the message, A single server tick took 60.00 seconds (should be max 0.05); Considering it to be crashed, server will forcibly shutdown. Once this criterion is met, it calls System.exit(1).

  <b>-1</b> - disable watchdog entirely (this disable option was added in 14w32a)
""")]
	[DisplayName("max-tick-time")]
	[DefaultValue(60000)]
	[Range(-1, long.MaxValue - 1)]
	public long? MaxTickTime { get; set; }

	[Description("This sets the maximum possible size in blocks, expressed as a radius, that the world border can obtain. Setting the world border bigger causes the commands to complete successfully but the actual border does not move past this block limit. Setting the max-world-size higher than the default doesn't appear to do anything.")]
	[DisplayName("max-world-size")]
	[DefaultValue(29999984)]
	[Range(1, 29999984)]
	public int? MaxWorldSize { get; set; }

	[Description("""
This is the message that is displayed in the server list of the client, below the name.
  The MOTD supports <a href="https://minecraft.fandom.com/wiki/Formatting_codes#Use_in_server.properties_and_pack.mcmeta" title="Formatting codes" target="_blank">color and formatting codes</a>.
  The MOTD supports special characters, such as "♥". However, such characters must be converted to escaped Unicode form. An online converter can be found <a target="_blank" rel="nofollow" class="external text" href="http://www.freeformatter.com/string-utilities.html#charinfo">here</a>.
  If the MOTD is over 59 characters, the server list may report a communication error.
""")]
	[DisplayName("motd")]
	[DefaultValue("A Minecraft Server")]
	public string? Motd { get; set; }

	[Description("""
By default it allows packets that are n-1 bytes big to go normally, but a packet of n bytes or more gets compressed down. So, a lower number means more compression but compressing small amounts of bytes might actually end up with a larger result than what went in.

  <b>-1</b> - disable compression entirely
  <b>0</b> - compress everything

Note: The Ethernet spec requires that packets less than 64 bytes become padded to 64 bytes. Thus, setting a value lower than 64 may not be beneficial. It is also not recommended to exceed the MTU, typically 1500 bytes.
""")]
	[DisplayName("network-compression-threshold")]
	[DefaultValue(256)]
	public int? NetworkCompressionThreshold { get; set; }

	[Description("""
Server checks connecting players against Minecraft account database. Set this to false only if the player's server is not connected to the Internet. Hackers with fake accounts can connect if this is set to false! If minecraft.net is down or inaccessible, no players can connect if this is set to true. Setting this variable to off purposely is called "cracking" a server, and servers that are present with online mode off are called "cracked" servers, allowing players with unlicensed copies of Minecraft to join.

  <b>true</b> - Enabled. The server assumes it has an Internet connection and checks every connecting player.
  <b>false</b> - Disabled. The server does not attempt to check connecting players.
""")]
	[DisplayName("online-mode")]
	[DefaultValue(true)]
	public bool? OnlineMode { get; set; }

	[Description("Sets the default permission level for ops when using <code>/op</code>.")]
	[DisplayName("op-permission-level")]
	[DefaultValue(4)]
	[Range(0, 4)]
	public int? OpPermissionLevel { get; set; }

	[Description("If non-zero, players are kicked from the server if they are idle for more than that many minutes.")]
	[DisplayName("player-idle-timeout")]
	[DefaultValue(0)]
	public int? PlayerIdleTimeout { get; set; }

	[Description("If the ISP/AS sent from the server is different from the one from Mojang Studios' authentication server, the player is kicked.")]
	[DisplayName("prevent-proxy-connections")]
	[DefaultValue(false)]
	public bool? PreventProxyConnections { get; set; }

	[Description("""
If set to true, chat preview will be enabled.

  <b>true</b> - Enabled. Server prevents users from using vpns or proxies.
  <b>false</b> - Disabled. The server doesn't prevent users from using vpns or proxies.
""")]
	[DisplayName("previews-chat")]
	[DefaultValue(false)]
	public bool? PreviewsChat { get; set; }

	[Description("""
Enable PvP on the server. Players shooting themselves with arrows receive damage only if PvP is enabled.

  <b>true</b> - Players can kill each other.
  <b>false</b> - Players cannot kill other players (also known as Player versus Environment (PvE)).

<b>Note:</b> Indirect damage sources spawned by players (such as lava, fire, TNT and to some extent water, sand and gravel) still deal damage to other players.
""")]
	[DisplayName("pvp")]
	[DefaultValue(true)]
	public bool? Pvp { get; set; }

	[Description("Sets the port for the query server (see enable-query).")]
	[DisplayName("query.port")]
	[DefaultValue(25565)]
	[Range(1, 65534)]
	public int? QueryPort { get; set; }

	[Description("Sets the maximum amount of packets a user can send before getting kicked. Setting to 0 disables this feature.")]
	[DisplayName("rate-limit")]
	[DefaultValue(0)]
	public int? RateLimit { get; set; }

	[Description("Sets the password for RCON: a remote console protocol that can allow other applications to connect and interact with a Minecraft server over the internet.")]
	[DisplayName("rcon.password")]
	[DefaultValue("")]
	public string? RconPassword { get; set; }

	[Description("Sets the RCON network port.")]
	[DisplayName("rcon.port")]
	[DefaultValue(25575)]
	[Range(1, 65534)]
	public int? RconPort { get; set; }

	[Description("""
Optional URI to a resource pack. The player may choose to use it.
Note that (in some versions before 1.15.2), the ":" and "=" characters need to be escaped with a backslash (\), e.g. http\://somedomain.com/somepack.zip?someparam\=somevalue

The resource pack may not have a larger file size than 250 MiB (Before 1.18: 100 MiB (≈ 100.8 MB)) (Before 1.15: 50 MiB (≈ 50.4 MB)). Note that download success or failure is logged by the client, and not by the server.
""")]
	[DisplayName("resource-pack")]
	[DefaultValue("")]
	public string? ResourcePack { get; set; }

	[Description("Optional, adds a custom message to be shown on resource pack prompt when <code>require-resource-pack</code> is used.")]
	[DisplayName("resource-pack-prompt")]
	[DefaultValue("")]
	public string? ResourcePackPrompt { get; set; }

	[Description("""
Optional SHA-1 digest of the resource pack, in lowercase hexadecimal. It is recommended to specify this, because it is used to verify the integrity of the resource pack.

<b>Note:</b> If the resource pack is any different, a yellow message "Invalid sha1 for resource-pack-sha1" appears in the console when the server starts. Due to the nature of hash functions, errors have a tiny probability of occurring, so this consequence has no effect.
""")]
	[DisplayName("resource-pack-sha1")]
	[DefaultValue("")]
	public string? ResourcePackSha1 { get; set; }

	[Description("When this option is enabled (set to true), players will be prompted for a response and will be disconnected if they decline the required pack.")]
	[DisplayName("require-resource-pack")]
	[DefaultValue(false)]
	public bool? RequireResourcePack { get; set; }

	[Description("The player should set this if they want the server to bind to a particular IP. It is strongly recommended that the player leaves server-ip blank. Set to blank, or the IP the player want their server to run (listen) on.")]
	[DisplayName("server-ip")]
	[DefaultValue("")]
	public string? ServerIp { get; set; }

	[Description("Changes the port the server is hosting (listening) on. This port must be forwarded if the server is hosted in a network using NAT (if the player has a home router/firewall).")]
	[DisplayName("server-port")]
	[DefaultValue(25565)]
	[Range(1, 65534)]
	public int? ServerPort { get; set; }

	[Description("""
Sets the maximum distance from players that living entities may be located in order to be updated by the server, measured in chunks in each direction of the player (radius, not diameter). If entities are outside of this radius, then they will not be ticked by the server nor will they be visible to players.

10 is the default/recommended. If the player has major lag, this value is recommended to be reduced.
""")]
	[DisplayName("simulation-distance")]
	[DefaultValue(10)]
	[Range(3, 32)]
	public int? SimulationDistance { get; set; }

	[Description("""
Sets whether the server sends snoop data regularly to <a target="_blank" rel="nofollow" class="external free" href="http://snoop.minecraft.net">http://snoop.minecraft.net</a>.
  <b>false</b> - disable snooping.
  <b>true</b> - enable snooping.
""")]
	[DisplayName("snooper-enabled")]
	[DefaultValue(true)]
	public bool? SnooperEnabled { get; set; }

	[Description("""
Determines if animals can spawn.

  <b>true</b> - Animals spawn as normal.
  <b>false</b> - Animals immediately vanish.

If the player has major lag, it is recommended to turn this off/set to false.
""")]
	[DisplayName("spawn-animals")]
	[DefaultValue(true)]
	public bool? SpawnAnimals { get; set; }

	[Description("""
Determines if monsters can spawn.

  <b>true</b> - Enabled. Monsters appear at night and in the dark.
  <b>false</b> - Disabled. No monsters.

This setting has no effect if difficulty = 0 (peaceful). If difficulty is not = 0, a monster can still spawn from a monster spawner.

If the player has major lag, it is recommended to turn this off/set to false.
""")]
	[DisplayName("spawn-monsters")]
	[DefaultValue(true)]
	public bool? SpawnMonsters { get; set; }

	[Description("""
Determines whether villagers can spawn.

  <b>true</b> - Enabled. Villagers spawn.
  <b>false</b> - Disabled. No villagers.
""")]
	[DisplayName("spawn-npcs")]
	[DefaultValue(true)]
	public bool? SpawnNpcs { get; set; }

	[Description("Determines the side length of the square spawn protection area as 2x+1. Setting this to 0 disables the spawn protection. A value of 1 protects a 3x3 square centered on the spawn point. 2 protects 5x5, 3 protects 7x7, etc. This option is not generated on the first server start and appears when the first player joins. If there are no ops set on the server, the spawn protection is disabled automatically as well.")]
	[DisplayName("spawn-protection")]
	[DefaultValue(16)]
	public int? SpawnProtection { get; set; }

	[Description("Enables synchronous chunk writes.")]
	[DisplayName("sync-chunk-writes")]
	[DefaultValue(true)]
	public bool? SyncChunkWrites { get; set; }

	[Description("""
Linux server performance improvements: optimized packet sending/receiving on Linux

  <b>true</b> - Enabled. Enable Linux packet sending/receiving optimization
  <b>false</b> - Disabled. Disable Linux packet sending/receiving optimization
""")]
	[DisplayName("use-native-transport")]
	[DefaultValue(true)]
	public bool? UseNativeTransport { get; set; }

	[Description("""
Sets the amount of world data the server sends the client, measured in chunks in each direction of the player (radius, not diameter). It determines the server-side viewing distance.

10 is the default/recommended. If the player has major lag, this value is recommended to be reduced.
""")]
	[DisplayName("view-distance")]
	[DefaultValue(10)]
	[Range(3, 32)]
	public int? ViewDistance { get; set; }

	[Description("""
Enables a whitelist on the server.
With a whitelist enabled, users not on the whitelist cannot connect. Intended for private servers, such as those for real-life friends or strangers carefully selected via an application process, for example.

  <b>false</b> - No white list is used.
  <b>true</b> - The file whitelist.json is used to generate the white list.

<b>Note:</b> Ops are automatically whitelisted, and there is no need to add them to the whitelist.
""")]
	[DisplayName("white-list")]
	[DefaultValue(false)]
	public bool? WhiteList { get; set; }
}
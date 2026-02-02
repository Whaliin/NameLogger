using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.UserMessages;
using Microsoft.Extensions.Logging;

namespace NameLogger;

public class Main : BasePlugin
{
    public override string ModuleName => "NameLogger";

    public override string ModuleVersion => "0.0.1";

    public override string ModuleDescription => "Fixes chat issues in Counter-Strike.";

    public Dictionary<uint, string> connectedNames = new();
    public List<CCSPlayerController> playerCache = new();

    /// <summary>
    /// Corrects the name of the given player if it does not match the stored correct name.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>True if the name was corrected, false otherwise.</returns>
    public void LogNameChange(CCSPlayerController player, string? nameOverride = null)
    {
        if (connectedNames.TryGetValue(player.Index, out var correctName) && (nameOverride != null ? nameOverride != correctName : player.PlayerName != correctName))
        {
            Server.PrintToChatAll(string.Format("'{0}' has changed their name to '{1}'", correctName, nameOverride ?? player.PlayerName));
            connectedNames[player.Index] = player.PlayerName;
        }
    }

    public HookResult OnMessage(UserMessage um)
    {
        if (Utilities.GetPlayerFromIndex(um.ReadInt("entityindex")) is not CCSPlayerController player || player.IsBot)
            return HookResult.Continue;

        var playerName = um.ReadString("param1");
        LogNameChange(player, playerName);

        return HookResult.Continue;
    }

/* 
    //Test function for changing names
    [ConsoleCommand("changename")]
    public void ChangeName(CCSPlayerController player, CommandInfo cmd)
    {
        if (player == null)
            return;

        // Create a random string
        var randomName = Guid.NewGuid().ToString("N")[..8];

        player.PlayerName = randomName;
        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
    }
*/

    [GameEventHandler]
    public HookResult OnPlayerConnectedFull(EventPlayerConnectFull evt, GameEventInfo info)
    {
        if (evt.Userid?.PlayerName == null)
            return HookResult.Continue;

        playerCache = Utilities.GetPlayers();

        connectedNames[evt.Userid.Index] = evt.Userid.PlayerName;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnected(EventPlayerDisconnect evt, GameEventInfo info)
    {
        if (evt.Userid == null)
            return HookResult.Continue;

        playerCache = Utilities.GetPlayers();

        connectedNames.Remove(evt.Userid.Index);
        return HookResult.Continue;
    }

    public void CheckNames()
    {
        foreach (var player in playerCache)
        {
            if (player.IsBot)
                continue;

            LogNameChange(player);
        }
    }

    public Main()
    {
        HookUserMessage(118, OnMessage, HookMode.Pre);

        AddTimer(5f, CheckNames);
    }

    public override void Load(bool hotReload)
    {
        playerCache = Utilities.GetPlayers();
        foreach (var player in playerCache)
        {
            connectedNames[player.Index] = player.PlayerName;
        }

        base.Load(hotReload);
    }
}

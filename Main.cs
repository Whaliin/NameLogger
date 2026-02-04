using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.UserMessages;

#if DEBUG
using CounterStrikeSharp.API.Modules.Commands;
#endif

namespace NameLogger;

public class Main : BasePlugin
{
    public override string ModuleName => "NameLogger";

    public override string ModuleVersion => "0.0.2";

    public override string ModuleDescription => "Logs messages when players change their name.";

    public Dictionary<uint, string> connectedNames = new();

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

    [GameEventHandler]
    public HookResult OnPlayerConnectedFull(EventPlayerConnectFull evt, GameEventInfo info)
    {
        if (evt.Userid?.PlayerName == null)
            return HookResult.Continue;

        connectedNames[evt.Userid.Index] = evt.Userid.PlayerName;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnected(EventPlayerDisconnect evt, GameEventInfo info)
    {
        if (evt.Userid == null)
            return HookResult.Continue;

        connectedNames.Remove(evt.Userid.Index);
        return HookResult.Continue;
    }

    public void CheckNames()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player.IsBot)
                continue;

            LogNameChange(player);
        }
    }

    public override void Load(bool hotReload)
    {
        HookUserMessage(118, OnMessage, HookMode.Pre);

        AddTimer(5f, CheckNames, TimerFlags.REPEAT);

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                connectedNames[player.Index] = player.PlayerName;
            }
        }

        base.Load(hotReload);
    }

#if DEBUG
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
#endif
}

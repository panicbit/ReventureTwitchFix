using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Models.Internal;

namespace ReventureTwitchFix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Harmony.CreateAndPatchAll(typeof(Plugin));
    }

    [HarmonyPatch(typeof(TwitchClient), "HandleIrcMessage")]
    [HarmonyFinalizer]
    static void HandleIrcMessage(Exception __exception)
    {
        if (__exception != null)
        {
            Logger.LogError(">>> Exception caught: " + __exception.ToString());
        }
    }

    [HarmonyPatch(typeof(TwitchClient), "HandlePrivMsg")]
    [HarmonyPrefix]
    static void HandlePrivMsg(IrcMessage ircMessage)
    {
        if (ircMessage.Hostmask.Equals("jtv!jtv@jtv.tmi.twitch.tv"))
        {
            return;
        }

        ircMessage.Tags.Remove("emotes");
    }
}
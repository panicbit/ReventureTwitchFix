using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        harmony.PatchAll(typeof(Plugin));
        harmony.PatchAll(typeof(EmotePatch));
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
    static void HandlePrivMsgPrefix(IrcMessage ircMessage)
    {
        if (ircMessage.Hostmask.Equals("jtv!jtv@jtv.tmi.twitch.tv"))
        {
            return;
        }

        string emotesTag;

        ircMessage.Tags.TryGetValue("emotes", out emotesTag);

        if (string.IsNullOrEmpty(emotesTag))
        {
            return;
        }

        var newEmotesTagParts = emotesTag.Split('/').Select(emoteStr =>
        {
            var parts = emoteStr.Split(':');
            var id = parts[0];
            var indices = parts[1];
            int newId;

            if (!int.TryParse(id, out newId))
            {
                newId = EmotePatch.getFakeId(id);
            }

            return $"{newId}:{indices}";
        });
        var newEmotesTag = string.Join("/", newEmotesTagParts);

        ircMessage.Tags["emotes"] = newEmotesTag;

        Logger.LogInfo(">>> emotes tag: " + emotesTag);
    }
}

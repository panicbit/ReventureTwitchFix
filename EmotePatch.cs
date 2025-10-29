using System;
using System.Collections.Generic;
using HarmonyLib;
using TwitchLib.Client.Models;

namespace ReventureTwitchFix;

class EmotePatch
{

    private static int idCounter = int.MinValue;
    private static readonly Dictionary<int, string> ToStrId = [];
    private static readonly Dictionary<string, int> ToIntId = [];

    public static int getFakeId(string strId)
    {
        int fakeId;

        if (ToIntId.TryGetValue(strId, out fakeId))
        {
            return fakeId;
        }

        if (idCounter >= 0)
        {
            throw new Exception("Exceeded negative emote id range");
        }

        fakeId = idCounter;
        idCounter += 1;
        ToIntId[strId] = fakeId;
        ToStrId[fakeId] = strId;

        return fakeId;
    }

    [HarmonyPatch(typeof(EmoteSet.Emote), "ImageUrl", MethodType.Getter)]
    [HarmonyPrefix]
    static bool RedirectImageUrl(ref string __result, ref EmoteSet.Emote __instance)
    {
        if (__instance.Id >= 0)
        {
            return true;
        }

        string id;

        if (!ToStrId.TryGetValue(__instance.Id, out id))
        {
            var chrisWOW = 1203895;
            __result = $"https://static-cdn.jtvnw.net/emoticons/v1/{chrisWOW}/1.0";

            Plugin.Logger.LogError($">>> Failed to resolve emote id {__instance.Id}");

            return false;
        }

        __result = $"https://static-cdn.jtvnw.net/emoticons/v1/{id}/1.0";

        return false;
    }
}
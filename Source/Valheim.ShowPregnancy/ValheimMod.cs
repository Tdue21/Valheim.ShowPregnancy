// ****************************************************************************
// * The MIT License(MIT)
// * Copyright © 2022 DoveSoft
// *
// * Permission is hereby granted, free of charge, to any person obtaining a
// * copy of this software and associated documentation files (the “Software”),
// * to deal in the Software without restriction, including without limitation
// * the rights to use, copy, modify, merge, publish, distribute, sublicense,
// * and/or sell copies of the Software, and to permit persons to whom the
// * Software is furnished to do so, subject to the following conditions:
// *
// * The above copyright notice and this permission notice shall be included in
// * all copies or substantial portions of the Software.
// *
// * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS
// * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL
// * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// * IN THE SOFTWARE.
// ****************************************************************************

using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace DoveSoft.Valheim.ShowPregnancy
{
    [BepInPlugin(ModUid, ModDescription, ModVersion)]
    [HarmonyPatch]
    public class ValheimMod : BaseUnityPlugin
    {
        internal const string ModVersion = "1.0.2";
        internal const string ModDescription = "Show Pregnancy";
        internal const string ModUid = "dovesoft.valheim.showpregnancy";

        private static ConfigEntry<bool> _enableMod;
        private static ConfigEntry<bool> _showPregnancy;
        private static ConfigEntry<bool> _showPregnancyProgress;
        private static ConfigEntry<bool> _showLovePoints;
        private static ConfigEntry<string> _customAnimals;
        private static ConfigEntry<bool> _showGrowth;

        private static readonly string[] VanillaAnimals = { "Lox(Clone)", "Boar(Clone)", "Wolf(Clone)" };
        private static readonly string[] VanillaBabies = { "Lox_Calf(Clone)", "Boar_Piggy(Clone)", "Wolf_Cub(Clone)" };

        private void Awake()
        {
            _enableMod             = Config.Bind("Global" , "Enable Mod"             , true,  "Enable or disable this mod.");
            _showPregnancy         = Config.Bind("General", "Show Pregnancy"         , true,  "Show or hide pregnancy status.");
            _showPregnancyProgress = Config.Bind("General", "Show Pregnancy Progress", false, "Show or hide pregnancy progress in percent.");
            _showLovePoints        = Config.Bind("General", "Show Love points"       , false, "Show or hide love points.");
            _customAnimals         = Config.Bind("General", "Custom Animals"         , ""   , "Add custom animals prefab ('<prefabName>(Clone)' and separated by ,) [i.e rae_OdinHorse(Clone),CustomAnimal(Clone)]");
            _showGrowth            = Config.Bind("General", "Show Baby Growth"       , true,  "Show or hide growth percentage of babies.");

            Config.Bind("General", "NexusID", 1787, "Nexus mod ID for updates");

            if (!_enableMod.Value)
            {
                return;
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
        public static string CharacterShowPregnancyText(string __result, Character __instance)
        {
            var customAnimals = _customAnimals.Value.Split(',');
            var breedableAnimals = VanillaAnimals.Concat(customAnimals).ToArray();
            var hoverText = (string)__result.Clone();

            if (__instance == null)
            {
                return hoverText;
            }

            var growup = __instance.GetGrowup();
            if (growup != null && _showGrowth.Value)
            {
                var spawned = growup.GetComponent<BaseAI>().GetTimeSinceSpawned().TotalSeconds;
                var total = growup.m_growTime;

                var grown = (spawned / total) * 100;

#if DEBUG
                UnityEngine.Debug.Log($"Looking at: {__instance.name} - Grown = {grown} - HoverText = {hoverText} .. ");
#endif

                hoverText += $"{Math.Round(grown, 0)}% grown";
                return hoverText;
            }


            if (!breedableAnimals.Contains(__instance.name) || !__instance.IsTamed())
            {
                return hoverText;
            }

            if (_showPregnancy.Value)
            {
                var duration = __instance.GetPregnancyDuration();
                var pregnancy = __instance.GetPregnancy();

                if (pregnancy != 0L && duration != 0)
                {
                    var timeGone = (ZNet.instance.GetTime() - new DateTime(pregnancy)).TotalSeconds;
                    var percentage = Math.Round(timeGone / duration * 100, 0);

                    hoverText = hoverText.Replace(" )", _showPregnancyProgress.Value 
                                                            ?  $", Pregnant ({percentage}%) )"
                                                            : ", Pregnant )");
#if DEBUG
                    UnityEngine.Debug.Log($"Show Pregnancy: {__instance.name} Duration={duration}s; Time Gone={timeGone}s; Percentage={percentage:N0}%");
#endif
                }
            }

            if (_showLovePoints.Value)
            {
                hoverText += Environment.NewLine + $"Lovepoints: {__instance.GetLovePoints()} of {__instance.GetRequiredLovePoints()}";
            }
            return hoverText;
        }
    }
}
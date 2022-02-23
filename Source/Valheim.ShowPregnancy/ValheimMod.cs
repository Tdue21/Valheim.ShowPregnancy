﻿// ****************************************************************************
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
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace DoveSoft.Valheim.ShowPregnancy
{
    [BepInPlugin(ModUid, "Show Pregnancy", ModVersion)]
    [HarmonyPatch]
    public class ValheimMod : BaseUnityPlugin
    {
        public const string ModVersion = "1.0.0";
        private const string ModUid = "dovesoft.valheim.showpregnancy";

        private static ConfigEntry<bool> _enableMod;
        private static ConfigEntry<bool> _showPregnancy;
        private static ConfigEntry<bool> _showPregnancyProgress;
        private static ConfigEntry<bool> _showLovePoints;

        private void Awake()
        {
            _enableMod             = Config.Bind("1 - Global",  "Enable Mod",              true,  "Enable or disable this mod.");
            _showPregnancy         = Config.Bind("2 - General", "Show Pregnancy",          true,  "Show or hide pregnancy status.");
            _showPregnancyProgress = Config.Bind("2 - General", "Show Pregnancy Progress", false, "Show or hide pregnancy progress in percent.");
            _showLovePoints        = Config.Bind("2 - General", "Show Love points",        false, "Show or hide love points.")       ;

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
            string[] validNames = { "Lox(Clone)", "Boar(Clone)", "Wolf(Clone)" };

            var hoverText = (string)__result.Clone();
            if (__instance == null)
            {
                return hoverText;
            }

            if (!validNames.Contains(__instance.name))
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
                    Debug.Log($"Show Pregnancy: Duration={duration}s; Time Gone={timeGone}s; Percentage={percentage:N0}%");
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
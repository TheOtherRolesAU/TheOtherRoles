using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(IntroCutscene.EMGDLDOHGCK), nameof(IntroCutscene.EMGDLDOHGCK.MoveNext))]
    class IntroPatch
    {
        // Intro special teams
        static void Prefix(IntroCutscene.EMGDLDOHGCK __instance)
        {
            if (PlayerControl.LocalPlayer == Jester.jester)
            {
                var jesterTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jesterTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jesterTeam;
            } else if (PlayerControl.LocalPlayer == Jackal.jackal) {
                var jackalTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jackalTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jackalTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && PlayerControl.LocalPlayer.PPMOEEPBHJO.FDNMBJOAPFL) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                foreach (PlayerControl p in players) {
                    if (p == Spy.spy || p.PPMOEEPBHJO.FDNMBJOAPFL)
                        fakeImpostorTeam.Add(p);
                }
                __instance.yourTeam = fakeImpostorTeam;
            }
        }

        // Intro display role
        static void Postfix(IntroCutscene.EMGDLDOHGCK __instance)
        {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            if (infos.Count == 0) return;
            RoleInfo roleInfo = infos[0];

            if (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2)
            {
                PlayerControl otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                __instance.__4__this.Title.text = PlayerControl.LocalPlayer.PPMOEEPBHJO.FDNMBJOAPFL ? "<color=#FF1919FF>Imp</color>Lover" : "Lover";
                __instance.__4__this.Title.color = PlayerControl.LocalPlayer.PPMOEEPBHJO.FDNMBJOAPFL ? Color.white : Lovers.color;
                __instance.__4__this.ImpostorText.text = "You are in <color=#FC03BEFF>Love</color><color=#FFFFFFFF> with </color><color=#FC03BEFF>" + (otherLover?.PPMOEEPBHJO?.PCLLABJCIPC ?? "") + "</color>";
                __instance.__4__this.ImpostorText.gameObject.SetActive(true);
                __instance.__4__this.BackgroundBar.material.color = Lovers.color;
            }
            else if (roleInfo.name == "Crewmate" || roleInfo.name == "Impostor") {}
            else {
                __instance.__4__this.Title.text = roleInfo.name;
                __instance.__4__this.Title.color = roleInfo.color;
                __instance.__4__this.ImpostorText.gameObject.SetActive(true);
                __instance.__4__this.ImpostorText.text = roleInfo.introDescription;
                __instance.__4__this.BackgroundBar.material.color = roleInfo.color;
            }
        }
    }
}

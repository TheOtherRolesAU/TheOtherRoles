using HarmonyLib;
using System;
using static BonusRoles.BonusRoles;
using UnityEngine;


namespace BonusRoles
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroPatch
    {
        // Intro special teams
        static void Prefix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer == Jester.jester)
            {
                var jesterTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jesterTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jesterTeam;
            } else if (PlayerControl.LocalPlayer == Shifter.shifter) {
                var shifterTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                shifterTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = shifterTeam;
            }
        }

        // Intro display role
        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer == Jester.jester)
            {
                __instance.__this.Title.Text = "Jester";
                __instance.__this.Title.Color = Jester.color;
                __instance.__this.ImpostorText.Text = "Get voted off of the ship to win";
                __instance.__this.BackgroundBar.material.color = Jester.color;
            }
            else if (PlayerControl.LocalPlayer == Mayor.mayor)
            {
                __instance.__this.Title.Text = "Mayor";
                __instance.__this.Title.Color = Mayor.color;
                __instance.__this.ImpostorText.Text = "Your vote counts twice";
                __instance.__this.BackgroundBar.material.color = Mayor.color;   
            }
            else if (PlayerControl.LocalPlayer == Engineer.engineer)
            {
                __instance.__this.Title.Text = "Engineer";
                __instance.__this.Title.Color = Engineer.color;
                __instance.__this.ImpostorText.Text = "Maintain important systems on the ship";
                __instance.__this.BackgroundBar.material.color = Engineer.color;
            }
            else if (PlayerControl.LocalPlayer == Godfather.godfather)
            {
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.Title.Text = "Godfather";
                __instance.__this.Title.Color = Godfather.color;
                __instance.__this.ImpostorText.Text = "Kill all crewmates";
                __instance.__this.BackgroundBar.material.color = Godfather.color;
            }
            else if (PlayerControl.LocalPlayer == Mafioso.mafioso)
            {
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.Title.Text = "Mafioso";
                __instance.__this.Title.Color = Mafioso.color;
                __instance.__this.ImpostorText.Text = "Work with the [FF1919FF]Mafia[FFFFFFFF] to kill the crewmates";
                __instance.__this.BackgroundBar.material.color = Mafioso.color;
            }
            else if (PlayerControl.LocalPlayer == Janitor.janitor)
            {
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.Title.Text = "Janitor";
                __instance.__this.Title.Color = Janitor.color;
                __instance.__this.ImpostorText.Text = "Work with the [FF1919FF]Mafia[FFFFFFFF] by hiding dead bodies";
                __instance.__this.BackgroundBar.material.color = Janitor.color;
            }
            else if (PlayerControl.LocalPlayer == Morphling.morphling)
            {
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.Title.Text = "Morphling";
                __instance.__this.Title.Color = Morphling.color;
                __instance.__this.ImpostorText.Text = "Change your look to not get caught";
                __instance.__this.BackgroundBar.material.color = Morphling.color;
            }
            else if (PlayerControl.LocalPlayer == Camouflager.camouflager)
            {
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.Title.Text = "Camouflager";
                __instance.__this.Title.Color = Camouflager.color;
                __instance.__this.ImpostorText.Text = "Camouflage and kill the crewmates";
                __instance.__this.BackgroundBar.material.color = Camouflager.color;
            }
            else if (PlayerControl.LocalPlayer == Sheriff.sheriff)
            {
                __instance.__this.Title.Text = "Sheriff";
                __instance.__this.Title.Color = Sheriff.color;
                __instance.__this.ImpostorText.Text = "Shoot the [FF1919FF]Impostors";
                __instance.__this.BackgroundBar.material.color = Sheriff.color;
            }
            else if (PlayerControl.LocalPlayer == Lighter.lighter)
            {
                __instance.__this.Title.Text = "Lighter";
                __instance.__this.Title.Color = Lighter.color;
                __instance.__this.ImpostorText.Text = "Your light never goes out";
                __instance.__this.BackgroundBar.material.color = Lighter.color;
            }
            else if (PlayerControl.LocalPlayer == Detective.detective)
            {
                __instance.__this.Title.Text = "Detective";
                __instance.__this.Title.Color = Detective.color;
                __instance.__this.ImpostorText.Text = "Find the [FF1919FF]Impostors[FFFFFFFF] by examining footprints";
                __instance.__this.BackgroundBar.material.color = Detective.color;
            }
            else if (PlayerControl.LocalPlayer == TimeMaster.timeMaster)
            {
                __instance.__this.Title.Text = "Time Master";
                __instance.__this.Title.Color = TimeMaster.color;
                __instance.__this.ImpostorText.Text = "Rewind time to find the [FF1919FF]Impostors";
                __instance.__this.BackgroundBar.material.color = TimeMaster.color;
            }
            else if (PlayerControl.LocalPlayer == Medic.medic)
            {
                __instance.__this.Title.Text = "Medic";
                __instance.__this.Title.Color = Medic.color;
                __instance.__this.ImpostorText.Text = "Create a shield to protect a person";
                __instance.__this.BackgroundBar.material.color = Medic.color;
            }
            else if (PlayerControl.LocalPlayer == Shifter.shifter)
            {
                __instance.__this.Title.Text = "Shifter";
                __instance.__this.Title.Color = Shifter.color;
                __instance.__this.ImpostorText.Text = "Shift your role before the game ends";
                __instance.__this.BackgroundBar.material.color = Shifter.color;
            }
            else if (PlayerControl.LocalPlayer == Swapper.swapper)
            {
                __instance.__this.Title.Text = "Swapper";
                __instance.__this.Title.Color = Swapper.color;
                __instance.__this.ImpostorText.Text = "Swap votes to exile the [FF1919FF]Impostors";
                __instance.__this.BackgroundBar.material.color = Swapper.color;
            }
            else if (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2)
            {
                PlayerControl otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                __instance.__this.Title.Text = PlayerControl.LocalPlayer.Data.IsImpostor ? "[FF1919FF]Imp[FC03BEFF]Lover" : "Lover";
                __instance.__this.Title.Color = PlayerControl.LocalPlayer.Data.IsImpostor ? Color.white : Lovers.color;
                __instance.__this.ImpostorText.Text = "You are in [FC03BEFF]Love [FFFFFFFF] with [FC03BEFF]" + (otherLover?.Data?.PlayerName ?? "");
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.BackgroundBar.material.color = Lovers.color;
            }
            else if (PlayerControl.LocalPlayer == Seer.seer)
            {
                __instance.__this.Title.Text = "Seer";
                __instance.__this.Title.Color = Seer.color;
                __instance.__this.ImpostorText.Text = "Reveal the intentions of everyone on the ship";
                __instance.__this.BackgroundBar.material.color = Seer.color;
            }
            else if (PlayerControl.LocalPlayer == Spy.spy)
            {
                __instance.__this.Title.Text = "Spy";
                __instance.__this.Title.Color = Spy.color;
                __instance.__this.ImpostorText.Text = "Spy on everyone to find the [FF1919FF]Impostors";
                __instance.__this.BackgroundBar.material.color = Spy.color;
            }
            else if (PlayerControl.LocalPlayer == Child.child)
            {
                __instance.__this.Title.Text = "Child";
                __instance.__this.Title.Color = Child.color;
                __instance.__this.ImpostorText.Text = "No one will harm you";
                __instance.__this.BackgroundBar.material.color = Child.color;
            }
        }
    }
}

using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

namespace TheOtherRoles
{
    public class RoleInfo {
        public Color color;
        public string name;
        public string introDescription;
        public string shortDescription;
        public bool isGood;

        RoleInfo(Color color, string name, string introDescription, string shortDescription, bool isGood) {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.isGood = isGood;
        }

        public string colorHexString() {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]", ToByte(color.r), ToByte(color.g), ToByte(color.b), ToByte(color.a));
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }


        public static RoleInfo getRoleInfoForPlayer(PlayerControl p) {
            string n = "";
            string iDesc = "";
            string sDesc = "";
            bool g = true;
            Color c = Color.white;

            if (Jester.jester != null && p == Jester.jester) {
                n = "Jester";
                c = Jester.color;
                iDesc = "Get voted out";
                sDesc = iDesc;
                g = false;
            }
            else if (Mayor.mayor != null && p == Mayor.mayor) {
                n = "Mayor";   
                c = Mayor.color;
                iDesc = "Your vote counts twice";
                sDesc = iDesc;
            }
            else if (Engineer.engineer != null && p == Engineer.engineer) {
                n = "Engineer";   
                c = Engineer.color;
                iDesc = "Maintain important systems on the ship";
                sDesc = "Repair the ship";
            }
            else if (Sheriff.sheriff != null && p == Sheriff.sheriff) {
                n = "Sheriff";   
                c = Sheriff.color;
                iDesc = "Shoot the [FF1919FF]Impostors";
                sDesc = "Shoot the Impostors";
            }
            else if (Lighter.lighter != null && p == Lighter.lighter) {
                n = "Lighter";   
                c = Lighter.color;
                iDesc = "Your light never goes out";
                sDesc = iDesc;
            }
            else if (Godfather.godfather != null && p == Godfather.godfather) {
                n = "Godfather";   
                c = Godfather.color;
                iDesc = "Kill all Crewmates";
                sDesc = iDesc;
                g = false;
            }
            else if (Mafioso.mafioso != null && p == Mafioso.mafioso) {
                n = "Mafioso";   
                c = Mafioso.color;
                iDesc = "Work with the [FF1919FF]Mafia[] to kill the Crewmates";
                sDesc = "Kill all Crewmates";
                g = false;
            }
            else if (Janitor.janitor != null && p == Janitor.janitor) {
                n = "Janitor";
                c = Janitor.color;
                iDesc = "Work with the [FF1919FF]Mafia[] by hiding dead bodies";
                sDesc = "Hide dead bodies";
                g = false;
            }
            else if (Morphling.morphling != null && p == Morphling.morphling) {
                n = "Morphling";
                c = Morphling.color;
                iDesc = "Change your look to not get caught";
                sDesc = "Change your look";
                g = false;
            }
            else if (Camouflager.camouflager != null && p == Camouflager.camouflager) {
                n = "Camouflager";
                c = Camouflager.color;
                iDesc = "Camouflage and kill the Crewmates";
                sDesc = "Hide among others";
                g = false;
            }
            else if (Vampire.vampire != null && p == Vampire.vampire) {
                n = "Vampire";
                c = Vampire.color;
                iDesc = "Kill the Crewmates with your bites";
                sDesc = "Bite your enemies";
                g = false;
            }
            else if (Detective.detective != null && p == Detective.detective) {
                n = "Detective";
                c = Detective.color;
                iDesc = "Find the [FF1919FF]Impostors[] by examining footprints";
                sDesc = "Examine footprints";
            }
            else if (TimeMaster.timeMaster != null && p == TimeMaster.timeMaster) {
                n = "Time Master";
                c = TimeMaster.color;
                iDesc = "Rewind time to find the [FF1919FF]Impostors";
                sDesc = "Rewind time";
            }
            else if (Medic.medic != null && p == Medic.medic) {
                n = "Medic";
                c = Medic.color;
                iDesc = "Protect someone with your shield";
                sDesc = "Protect other players";
            }
            else if (Shifter.shifter != null && p == Shifter.shifter) {
                n = "Shifter";
                c = Shifter.color;
                iDesc = "Shift your role before the game ends";
                sDesc = "Shift your role";
                g = false;
            }
            else if (Swapper.swapper != null && p == Swapper.swapper) {
                n = "Swapper";
                c = Swapper.color;
                iDesc = "Swap votes to exile the [FF1919FF]Impostors";
                sDesc = "Swap votes";
            }
            else if ((Lovers.lover1 != null && p == Lovers.lover1) || (Lovers.lover2 != null && p == Lovers.lover2)) {
                n = p.Data.IsImpostor ? "ImpLover" : "Lover";
                c = p.Data.IsImpostor ? Palette.ImpostorRed : Lovers.color;
                iDesc = "You are in [FC03BEFF]Love[]";
                sDesc = "You are in love";
                g = !p.Data.IsImpostor;
            }
            else if (Seer.seer != null && p == Seer.seer) { 
                n = "Seer";
                c = Seer.color;
                iDesc = "Reveal the intentions of everyone on the ship";
                sDesc = "Reveal all roles";
            }
            else if (Spy.spy != null && p == Spy.spy) { 
                n = "Spy";
                c = Spy.color;
                iDesc = "Spy on everyone to find the [FF1919FF]Impostors";
                sDesc = "Spy on everyone";
            }
            else if (Child.child != null && p == Child.child) { 
                n = "Child";
                c = Child.color;
                iDesc = "No one will harm you until you grow up";
                sDesc = "No one will harm you";
            }
            else if (BountyHunter.bountyHunter != null && p == BountyHunter.bountyHunter) {
                n = "Bounty Hunter";
                c = BountyHunter.color;
                iDesc = "Hunt your bounty down";
                sDesc = "Hunt your bounty";
                g = false;
            }
            else if (Tracker.tracker != null && p == Tracker.tracker) {
                n = "Tracker";
                c = Tracker.color;
                iDesc = "Track the [FF1919FF]Impostors[] down";
                sDesc = "Track the Impostors down";
            }
            else if (Snitch.snitch != null && p == Snitch.snitch) {
                n = "Snitch";
                c = Snitch.color;
                iDesc = "Finish your tasks to find the [FF1919FF]Impostors[]";
                sDesc = "Finish your tasks";
            }
            else if (p.Data.IsImpostor) { // Just Impostor
                n = "Impostor";
                c = Palette.ImpostorRed;
                iDesc = "";
                sDesc = "Sabotage and kill everyone";
                g = false;
            }
            else { // Just Crewmate
                n = "Crewmate";
                c = Color.white;
                iDesc = "";
                sDesc = "Find the Impostors";
            }

            return new RoleInfo(
                c,
                n,
                iDesc,
                sDesc,
                g
            );
        }
    }
}
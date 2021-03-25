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
            string name = "";
            string introDescription = "";
            string shortDescription = "";
            bool isGood = true;
            Color color = Color.white;

            if (Jester.jester != null && p == Jester.jester) {
                name = "Jester";
                color = Jester.color;
                introDescription = "Get voted out";
                shortDescription = introDescription;
                isGood = false;
            }
            else if (Mayor.mayor != null && p == Mayor.mayor) {
                name = "Mayor";   
                color = Mayor.color;
                introDescription = "Your vote counts twice";
                shortDescription = introDescription;
            }
            else if (Engineer.engineer != null && p == Engineer.engineer) {
                name = "Engineer";   
                color = Engineer.color;
                introDescription = "Maintain important systems on the ship";
                shortDescription = "Repair the ship";
            }
            else if (Sheriff.sheriff != null && p == Sheriff.sheriff) {
                name = "Sheriff";   
                color = Sheriff.color;
                introDescription = "Shoot the [FF1919FF]Impostors";
                shortDescription = "Shoot the Impostors";
            }
            else if (Lighter.lighter != null && p == Lighter.lighter) {
                name = "Lighter";   
                color = Lighter.color;
                introDescription = "Your light never goes out";
                shortDescription = introDescription;
            }
            else if (Godfather.godfather != null && p == Godfather.godfather) {
                name = "Godfather";   
                color = Godfather.color;
                introDescription = "Kill all Crewmates";
                shortDescription = introDescription;
                isGood = false;
            }
            else if (Mafioso.mafioso != null && p == Mafioso.mafioso) {
                name = "Mafioso";   
                color = Mafioso.color;
                introDescription = "Work with the [FF1919FF]Mafia[] to kill the Crewmates";
                shortDescription = "Kill all Crewmates";
                isGood = false;
            }
            else if (Janitor.janitor != null && p == Janitor.janitor) {
                name = "Janitor";
                color = Janitor.color;
                introDescription = "Work with the [FF1919FF]Mafia[] by hiding dead bodies";
                shortDescription = "Hide dead bodies";
                isGood = false;
            }
            else if (Morphling.morphling != null && p == Morphling.morphling) {
                name = "Morphling";
                color = Morphling.color;
                introDescription = "Change your look to not get caught";
                shortDescription = "Change your look";
                isGood = false;
            }
            else if (Camouflager.camouflager != null && p == Camouflager.camouflager) {
                name = "Camouflager";
                color = Camouflager.color;
                introDescription = "Camouflage and kill the Crewmates";
                shortDescription = "Hide among others";
                isGood = false;
            }
            else if (Vampire.vampire != null && p == Vampire.vampire) {
                name = "Vampire";
                color = Vampire.color;
                introDescription = "Kill the Crewmates with your bites";
                shortDescription = "Bite your enemies";
                isGood = false;
            }
            else if (Detective.detective != null && p == Detective.detective) {
                name = "Detective";
                color = Detective.color;
                introDescription = "Find the [FF1919FF]Impostors[] by examining footprints";
                shortDescription = "Examine footprints";
            }
            else if (TimeMaster.timeMaster != null && p == TimeMaster.timeMaster) {
                name = "Time Master";
                color = TimeMaster.color;
                introDescription = "Rewind time to find the [FF1919FF]Impostors";
                shortDescription = "Rewind time";
            }
            else if (Medic.medic != null && p == Medic.medic) {
                name = "Medic";
                color = Medic.color;
                introDescription = "Protect someone with your shield";
                shortDescription = "Protect other players";
            }
            else if (Shifter.shifter != null && p == Shifter.shifter) {
                name = "Shifter";
                color = Shifter.color;
                introDescription = "Shift your role before the game ends";
                shortDescription = "Shift your role";
                isGood = false;
            }
            else if (Swapper.swapper != null && p == Swapper.swapper) {
                name = "Swapper";
                color = Swapper.color;
                introDescription = "Swap votes to exile the [FF1919FF]Impostors";
                shortDescription = "Swap votes";
            }
            else if (Seer.seer != null && p == Seer.seer) { 
                name = "Seer";
                color = Seer.color;
                introDescription = "Reveal the intentions of everyone on the ship";
                shortDescription = "Reveal all roles";
            }
            else if (Spy.spy != null && p == Spy.spy) { 
                name = "Spy";
                color = Spy.color;
                introDescription = "Spy on everyone to find the [FF1919FF]Impostors";
                shortDescription = "Spy on everyone";
            }
            else if (Child.child != null && p == Child.child) { 
                name = "Child";
                color = Child.color;
                introDescription = "No one will harm you until you grow up";
                shortDescription = "No one will harm you";
            }
            else if (BountyHunter.bountyHunter != null && p == BountyHunter.bountyHunter) {
                name = "Bounty Hunter";
                color = BountyHunter.color;
                introDescription = "Hunt your bounty down";
                shortDescription = "Hunt your bounty";
                isGood = false;
            }
            else if (Tracker.tracker != null && p == Tracker.tracker) {
                name = "Tracker";
                color = Tracker.color;
                introDescription = "Track the [FF1919FF]Impostors[] down";
                shortDescription = "Track the Impostors down";
            }
            else if (Snitch.snitch != null && p == Snitch.snitch) {
                name = "Snitch";
                color = Snitch.color;
                introDescription = "Finish your tasks to find the [FF1919FF]Impostors[]";
                shortDescription = "Finish your tasks";
            }
            else if (Jackal.jackal != null && p == Jackal.jackal) {
                name = "Jackal";
                color = Jackal.color;
                introDescription = "Kill all Crewmates and [FF1919FF]Impostors[FFFFFFFF] to win";
                shortDescription = "";
                isGood = false;                
            }
            else if (Sidekick.sidekick != null && p == Sidekick.sidekick) {
                name = "Sidekick";
                color = Sidekick.color;
                introDescription = "";
                shortDescription = "Help your Jackal to kill everyone";
                isGood = false;
            }
            else if ((Lovers.lover1 != null && p == Lovers.lover1) || (Lovers.lover2 != null && p == Lovers.lover2)) {
                name = p.Data.IsImpostor ? "ImpLover" : "Lover";
                color = p.Data.IsImpostor ? Palette.ImpostorRed : Lovers.color;
                introDescription = "You are in [FC03BEFF]Love[]";
                shortDescription = "You are in love";
                isGood = !p.Data.IsImpostor;
            }
            else if (p.Data.IsImpostor) { // Just Impostor
                name = "Impostor";
                color = Palette.ImpostorRed;
                introDescription = "";
                shortDescription = "Sabotage and kill everyone";
                isGood = false;
            }
            else { // Just Crewmate
                name = "Crewmate";
                color = Color.white;
                introDescription = "";
                shortDescription = "Find the Impostors";
            }

            return new RoleInfo(
                color,
                name,
                introDescription,
                shortDescription,
                isGood
            );
        }
    }
}
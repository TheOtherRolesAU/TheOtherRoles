using HarmonyLib;
using System.Linq;
using System;
using System.Collections.Generic;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

namespace TheOtherRoles
{
    class RoleInfo {
        public Color color;
        public string name;
        public string introDescription;
        public string shortDescription;
        public RoleId roleId;

        RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId) {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
        }

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p) {
            List<RoleInfo> infos = new List<RoleInfo>();

            // Special roles
            if (Jester.jester != null && p == Jester.jester) {
                infos.Add(new RoleInfo("Jester",
                Jester.color,
                "Get voted out",
                "Get voted out",
                RoleId.Jester));
            }
            if (Mayor.mayor != null && p == Mayor.mayor) {
                infos.Add(new RoleInfo("Mayor",
                Mayor.color,
                "Your vote counts twice",
                "Your vote counts twice",
                RoleId.Mayor));
            }
            if (Engineer.engineer != null && p == Engineer.engineer) {
                infos.Add(new RoleInfo("Engineer", 
                Engineer.color,
                "Maintain important systems on the ship",
                "Repair the ship",
                RoleId.Engineer));
            }
            if (Sheriff.sheriff != null && p == Sheriff.sheriff) {
                infos.Add(new RoleInfo("Sheriff",
                Sheriff.color,
                "Shoot the <color=#FF1919FF>Impostors</color>",
                "Shoot the Impostors",
                RoleId.Sheriff));
            }
            if (Lighter.lighter != null && p == Lighter.lighter) {
                infos.Add(new RoleInfo("Lighter",   
                Lighter.color,
                "Your light never goes out",
                "Your light never goes out",
                RoleId.Lighter));
            }
            if (Godfather.godfather != null && p == Godfather.godfather) {
                infos.Add(new RoleInfo("Godfather",
                Godfather.color,
                "Kill all Crewmates",
                "Kill all Crewmates",
                RoleId.Godfather));
                ;
            }
            if (Mafioso.mafioso != null && p == Mafioso.mafioso) {
                infos.Add(new RoleInfo("Mafioso",
                Mafioso.color,
                "Work with the <color=#FF1919FF>Mafia</color> to kill the Crewmates",
                "Kill all Crewmates",
                RoleId.Mafioso));
            }
            if (Janitor.janitor != null && p == Janitor.janitor) {
                infos.Add(new RoleInfo("Janitor",
                Janitor.color,
                "Work with the <color=#FF1919FF>Mafia</color> by hiding dead bodies",
                "Hide dead bodies",
                RoleId.Janitor));
            }
            if (Morphling.morphling != null && p == Morphling.morphling) {
                infos.Add(new RoleInfo("Morphling",
                Morphling.color,
                "Change your look to not get caught",
                "Change your look",
                RoleId.Morphling));
            }
            if (Camouflager.camouflager != null && p == Camouflager.camouflager) {
                infos.Add(new RoleInfo("Camouflager",
                Camouflager.color,
                "Camouflage and kill the Crewmates",
                "Hide among others",
                RoleId.Camouflager));
            }
            if (Vampire.vampire != null && p == Vampire.vampire) {
                infos.Add(new RoleInfo("Vampire",
                Vampire.color,
                "Kill the Crewmates with your bites",
                "Bite your enemies",
                RoleId.Vampire));
            }
            if (Eraser.eraser != null && p == Eraser.eraser) {
                infos.Add(new RoleInfo("Eraser",
                Eraser.color,
                "Kill the Crewmates and erase their roles",
                "Erase the roles of your enemies",
                RoleId.Eraser));
            }
            if (Trickster.trickster != null && p == Trickster.trickster) {
                infos.Add(new RoleInfo("Trickster",
                Trickster.color,
                "Use your jack-in-the-boxes to surprise others",
                "Surprise your enemies",
                RoleId.Trickster));
            }
            if (Cleaner.cleaner != null && p == Cleaner.cleaner) {
                infos.Add(new RoleInfo("Cleaner",
                Cleaner.color,
                "Kill everyone and leave no traces",
                "Clean up dead bodies",
                RoleId.Cleaner));
            }
            if (Warlock.warlock != null && p == Warlock.warlock) {
                infos.Add(new RoleInfo("Warlock",
                Warlock.color,
                "Curse other players and kill everyone",
                "Curse and kill everyone",
                RoleId.Warlock));
            }
            if (Detective.detective != null && p == Detective.detective) {
                infos.Add(new RoleInfo("Detective",
                Detective.color,
                "Find the <color=#FF1919FF>Impostors</color> by examining footprints",
                "Examine footprints",
                RoleId.Detective));
            }
            if (TimeMaster.timeMaster != null && p == TimeMaster.timeMaster) {
                infos.Add(new RoleInfo("Time Master",
                TimeMaster.color,
                "Save yourself with your time shield",
                "Use your time shield",
                RoleId.TimeMaster));
            }
            if (Medic.medic != null && p == Medic.medic) {
                infos.Add(new RoleInfo("Medic",
                Medic.color,
                "Protect someone with your shield",
                "Protect other players",
                RoleId.Medic));
            }
            if (Shifter.shifter != null && p == Shifter.shifter) {
                infos.Add(new RoleInfo("Shifter",
                Shifter.color,
                "Shift your role",
                "Shift your role",
                RoleId.Shifter));
            }
            if (Swapper.swapper != null && p == Swapper.swapper) {
                infos.Add(new RoleInfo("Swapper",
                Swapper.color,
                "Swap votes to exile the <color=#FF1919FF>Impostors</color>",
                "Swap votes",
                RoleId.Swapper));
            }
            if (Seer.seer != null && p == Seer.seer) { 
                infos.Add(new RoleInfo("Seer",
                Seer.color,
                "You will see players die",
                "You will see players die",
                RoleId.Seer));
            }
            if (Hacker.hacker != null && p == Hacker.hacker) { 
                infos.Add(new RoleInfo("Hacker",
                Hacker.color,
                "Hack to find the <color=#FF1919FF>Impostors</color>",
                "Hack to find the Impostors",
                RoleId.Hacker));
            }
            if (Child.child != null && p == Child.child) { 
                infos.Add(new RoleInfo(p.Data.IsImpostor ? "Bad Child" : "Good Child",
                p.Data.IsImpostor ? Palette.ImpostorRed : Child.color,
                "No one will harm you until you grow up",
                "No one will harm you",
                RoleId.Child));
            }
            if (Tracker.tracker != null && p == Tracker.tracker) {
                infos.Add(new RoleInfo("Tracker",
                Tracker.color,
                "Track the <color=#FF1919FF>Impostors</color> down",
                "Track the Impostors down",
                RoleId.Tracker));
            }
            if (Snitch.snitch != null && p == Snitch.snitch) {
                infos.Add(new RoleInfo("Snitch",
                Snitch.color,
                "Finish your tasks to find the <color=#FF1919FF>Impostors</color>",
                "Finish your tasks",
                RoleId.Snitch));
            }
            if ((Jackal.jackal != null && p == Jackal.jackal) || (Jackal.formerJackals != null && Jackal.formerJackals.Any(x => x.PlayerId == p.PlayerId))) {
                infos.Add(new RoleInfo("Jackal",
                Jackal.color,
                "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win",
                "Kill everyone",
                RoleId.Jackal));                
            }
            if (Sidekick.sidekick != null && p == Sidekick.sidekick) {
                infos.Add(new RoleInfo("Sidekick",
                Sidekick.color,
                "",
                "Help your Jackal to kill everyone",
                RoleId.Sidekick));
            }
            if (Spy.spy != null && p == Spy.spy) {
                infos.Add(new RoleInfo("Spy",
                Spy.color,
                "Confuse the <color=#FF1919FF>Impostors</color>",
                "Confuse the Impostors",
                RoleId.Spy));
            }
            if (SecurityGuard.securityGuard != null && p == SecurityGuard.securityGuard) {
                infos.Add(new RoleInfo("Security Guard",
                SecurityGuard.color,
                "Seal vents and place cameras",
                "Seal vents and place cameras",
                RoleId.SecurityGuard));
            }
            if (Arsonist.arsonist != null && p == Arsonist.arsonist) {
                infos.Add(new RoleInfo("Arsonist",
                Arsonist.color,
                "Let them burn",
                "Let them burn",
                RoleId.Arsonist));
            }

            // Default roles
            if (infos.Count == 0 && p.Data.IsImpostor) { // Just Impostor
                infos.Add(new RoleInfo("Impostor",
                Palette.ImpostorRed,
                Helpers.cs(Palette.ImpostorRed, "Sabotage and kill everyone"),
                "Sabotage and kill everyone",
                RoleId.Impostor));
            } else if (infos.Count == 0) { // Just Crewmate
                infos.Add(new RoleInfo("Crewmate",
                Color.white,
                "Find the Impostors",
                "Find the Impostors",
                RoleId.Crewmate));
            }

            // Modifier
            if ((Lovers.lover1 != null && p == Lovers.lover1) || (Lovers.lover2 != null && p == Lovers.lover2)) {
                infos.Add(new RoleInfo("Lover",
                Lovers.color,
                "You are in <color=#FC03BEFF>Love</color>",
                "You are in love",
                RoleId.Lover));
            }

            return infos;
        }
    }
}

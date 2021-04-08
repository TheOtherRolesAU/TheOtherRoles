using HarmonyLib;
using System;
using System.Collections.Generic;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;

using Palette = GLNPIJPGGNJ;

namespace TheOtherRoles
{
    public class RoleInfo {
        public Color color;
        public string name;
        public string introDescription;
        public string shortDescription;

        RoleInfo(string name, Color color, string introDescription, string shortDescription) {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
        }

        public string colorHexString() {
            return string.Format("[{0:X2}{1:X2}{2:X2}{3:X2}]", ToByte(color.r), ToByte(color.g), ToByte(color.b), ToByte(color.a));
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p) {
            List<RoleInfo> infos = new List<RoleInfo>();

            if (Jester.jester != null && p == Jester.jester) {
                infos.Add(new RoleInfo("Jester",
                Jester.color,
                "Get voted out",
                "Get voted out"));
            }
            if (Mayor.mayor != null && p == Mayor.mayor) {
                infos.Add(new RoleInfo("Mayor",
                Mayor.color,
                "Your vote counts twice",
                "Your vote counts twice"));
            }
            if (Engineer.engineer != null && p == Engineer.engineer) {
                infos.Add(new RoleInfo("Engineer", 
                Engineer.color,
                "Maintain important systems on the ship",
                "Repair the ship"));
            }
            if (Sheriff.sheriff != null && p == Sheriff.sheriff) {
                infos.Add(new RoleInfo("Sheriff",
                Sheriff.color,
                "Shoot the [FF1919FF]Impostors",
                "Shoot the Impostors"));
            }
            if (Lighter.lighter != null && p == Lighter.lighter) {
                infos.Add(new RoleInfo("Lighter",   
                Lighter.color,
                "Your light never goes out",
                "Your light never goes out"));
            }
            if (Godfather.godfather != null && p == Godfather.godfather) {
                infos.Add(new RoleInfo("Godfather",
                Godfather.color,
                "Kill all Crewmates",
                "Kill all Crewmates"));
                ;
            }
            if (Mafioso.mafioso != null && p == Mafioso.mafioso) {
                infos.Add(new RoleInfo("Mafioso",
                Mafioso.color,
                "Work with the [FF1919FF]Mafia[] to kill the Crewmates",
                "Kill all Crewmates"));
            }
            if (Janitor.janitor != null && p == Janitor.janitor) {
                infos.Add(new RoleInfo("Janitor",
                Janitor.color,
                "Work with the [FF1919FF]Mafia[] by hiding dead bodies",
                "Hide dead bodies"));
            }
            if (Morphling.morphling != null && p == Morphling.morphling) {
                infos.Add(new RoleInfo("Morphling",
                Morphling.color,
                "Change your look to not get caught",
                "Change your look"));
            }
            if (Camouflager.camouflager != null && p == Camouflager.camouflager) {
                infos.Add(new RoleInfo("Camouflager",
                Camouflager.color,
                "Camouflage and kill the Crewmates",
                "Hide among others"));
            }
            if (Vampire.vampire != null && p == Vampire.vampire) {
                infos.Add(new RoleInfo("Vampire",
                Vampire.color,
                "Kill the Crewmates with your bites",
                "Bite your enemies"));
            }
            if (Eraser.eraser != null && p == Eraser.eraser) {
                infos.Add(new RoleInfo("Eraser",
                Eraser.color,
                "Kill the Crewmates and erase their roles",
                "Erase the roles of your enemies"));
            }
            if (Detective.detective != null && p == Detective.detective) {
                infos.Add(new RoleInfo("Detective",
                Detective.color,
                "Find the [FF1919FF]Impostors[] by examining footprints",
                "Examine footprints"));
            }
            if (TimeMaster.timeMaster != null && p == TimeMaster.timeMaster) {
                infos.Add(new RoleInfo("Time Master",
                TimeMaster.color,
                "Save yourself with your time shield",
                "Use your time shield"));
            }
            if (Medic.medic != null && p == Medic.medic) {
                infos.Add(new RoleInfo("Medic",
                Medic.color,
                "Protect someone with your shield",
                "Protect other players"));
            }
            if (Shifter.shifter != null && p == Shifter.shifter) {
                infos.Add(new RoleInfo("Shifter",
                Shifter.color,
                "Shift your role",
                "Shift your role"));
            }
            if (Swapper.swapper != null && p == Swapper.swapper) {
                infos.Add(new RoleInfo("Swapper",
                Swapper.color,
                "Swap votes to exile the [FF1919FF]Impostors",
                "Swap votes"));
            }
            if (Seer.seer != null && p == Seer.seer) { 
                infos.Add(new RoleInfo("Seer",
                Seer.color,
                "You will see players die",
                "You will see players die"));
            }
            if (Hacker.hacker != null && p == Hacker.hacker) { 
                infos.Add(new RoleInfo("Hacker",
                Hacker.color,
                "Hacker to find the [FF1919FF]Impostors",
                "Hacker to find the Impostors"));
            }
            if (Child.child != null && p == Child.child) { 
                infos.Add(new RoleInfo(p.IDOFAMCIJKE.CIDDOFDJHJH ? "Bad Child" : "Good Child",
                p.IDOFAMCIJKE.CIDDOFDJHJH ? Palette.LDCHDOFJPGH : Child.color,
                "No one will harm you until you grow up",
                "No one will harm you"));
            }
            if (Tracker.tracker != null && p == Tracker.tracker) {
                infos.Add(new RoleInfo("Tracker",
                Tracker.color,
                "Track the [FF1919FF]Impostors[] down",
                "Track the Impostors down"));
            }
            if (Snitch.snitch != null && p == Snitch.snitch) {
                infos.Add(new RoleInfo("Snitch",
                Snitch.color,
                "Finish your tasks to find the [FF1919FF]Impostors[]",
                "Finish your tasks"));
            }
            if (Jackal.jackal != null && p == Jackal.jackal) {
                infos.Add(new RoleInfo("Jackal",
                Jackal.color,
                "Kill all Crewmates and [FF1919FF]Impostors[] to win",
                "Kill everyone"));                
            }
            if (Sidekick.sidekick != null && p == Sidekick.sidekick) {
                infos.Add(new RoleInfo("Sidekick",
                Sidekick.color,
                "",
                "Help your Jackal to kill everyone"));
            }
            if ((Lovers.lover1 != null && p == Lovers.lover1) || (Lovers.lover2 != null && p == Lovers.lover2)) {
                infos.Add(new RoleInfo(p.IDOFAMCIJKE.CIDDOFDJHJH ? "ImpLover" : "Lover",
                p.IDOFAMCIJKE.CIDDOFDJHJH ? Palette.LDCHDOFJPGH : Lovers.color,
                "You are in [FC03BEFF]Love[]",
                "You are in love"));
            }
            if (Spy.spy != null && p == Spy.spy) {
                infos.Add(new RoleInfo("Spy",
                Spy.color,
                "Confuse the [FF1919FF]Impostors[]",
                "Confuse the Impostors"));
            }
            if (infos.Count == 0 && p.IDOFAMCIJKE.CIDDOFDJHJH) { // Just Impostor
                infos.Add(new RoleInfo("Impostor",
                Palette.LDCHDOFJPGH,
                "",
                "Sabotage and kill everyone"));
            } else if (infos.Count == 0) { // Just Crewmate
                infos.Add(new RoleInfo("Crewmate",
                Color.white,
                "",
                "Find the Impostors"));
            }

            return infos;
        }
    }
}
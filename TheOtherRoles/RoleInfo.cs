using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Roles;
using UnityEngine;

namespace TheOtherRoles
{
    internal class RoleInfo
    {
        public static readonly RoleInfo Jester =
            new("Jester", Roles.Jester.Instance.color, "Get voted out", "Get voted out",
                RoleId.Jester);

        public static readonly RoleInfo Mayor = new("Mayor", Roles.Mayor.Instance.color,
            "Your vote counts twice",
            "Your vote counts twice", RoleId.Mayor);

        public static readonly RoleInfo Engineer = new("Engineer", Roles.Engineer.Instance.color,
            "Maintain important systems on the ship", "Repair the ship", RoleId.Engineer);

        public static readonly RoleInfo Sheriff = new("Sheriff", Roles.Sheriff.Instance.color,
            "Shoot the <color=#FF1919FF>Impostors</color>", "Shoot the Impostors", RoleId.Sheriff);

        public static readonly RoleInfo Lighter = new("Lighter", Roles.Lighter.Instance.color,
            "Your light never goes out",
            "Your light never goes out", RoleId.Lighter);

        public static readonly RoleInfo Godfather = new("Godfather", Roles.Godfather.Instance.color,
            "Kill all Crewmates",
            "Kill all Crewmates", RoleId.Godfather);

        public static readonly RoleInfo Mafioso = new("Mafioso", Roles.Mafioso.Instance.color,
            "Work with the <color=#FF1919FF>Mafia</color> to kill the Crewmates", "Kill all Crewmates", RoleId.Mafioso);

        public static readonly RoleInfo Janitor = new("Janitor", Roles.Janitor.Instance.color,
            "Work with the <color=#FF1919FF>Mafia</color> by hiding dead bodies", "Hide dead bodies", RoleId.Janitor);

        public static readonly RoleInfo Morphling = new("Morphling",
            Roles.Morphling.Instance.color,
            "Change your look to not get caught", "Change your look", RoleId.Morphling);

        public static readonly RoleInfo Camouflager = new("Camouflager",
            Roles.Camouflager.Instance.color,
            "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager);

        public static readonly RoleInfo Vampire = new("Vampire", Roles.Vampire.Instance.color,
            "Kill the Crewmates with your bites",
            "Bite your enemies", RoleId.Vampire);

        public static readonly RoleInfo Eraser = new("Eraser", Roles.Eraser.Instance.color,
            "Kill the Crewmates and erase their roles",
            "Erase the roles of your enemies", RoleId.Eraser);

        public static readonly RoleInfo Trickster = new("Trickster",
            Roles.Trickster.Instance.color,
            "Use your jack-in-the-boxes to surprise others", "Surprise your enemies", RoleId.Trickster);

        public static readonly RoleInfo Cleaner = new("Cleaner", Roles.Cleaner.Instance.color,
            "Kill everyone and leave no traces",
            "Clean up dead bodies", RoleId.Cleaner);

        public static readonly RoleInfo Warlock = new("Warlock", Roles.Warlock.Instance.color,
            "Curse other players and kill everyone",
            "Curse and kill everyone", RoleId.Warlock);

        public static readonly RoleInfo BountyHunter = new("Bounty Hunter",
            Roles.BountyHunter.Instance.color, "Hunt your Bounty down",
            "Hunt your Bounty down", RoleId.BountyHunter);

        public static readonly RoleInfo Detective = new("Detective", Roles.Detective.Instance.color,
            "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Examine footprints",
            RoleId.Detective);

        public static readonly RoleInfo TimeMaster = new("Time Master",
            Roles.TimeMaster.Instance.color,
            "Save yourself with your time shield", "Use your time shield", RoleId.TimeMaster);

        public static readonly RoleInfo Medic = new("Medic", Roles.Medic.Instance.color,
            "Protect someone with your shield",
            "Protect other players", RoleId.Medic);

        public static readonly RoleInfo Shifter =
            new("Shifter", Roles.Shifter.Instance.color, "Shift your role", "Shift your role",
                RoleId.Shifter);

        public static readonly RoleInfo Swapper = new("Swapper", Roles.Swapper.Instance.color,
            "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper);

        public static readonly RoleInfo Seer = new("Seer", Roles.Seer.Instance.color, "You will see players die",
            "You will see players die", RoleId.Seer);

        public static readonly RoleInfo Hacker = new("Hacker", Roles.Hacker.Instance.color,
            "Hack systems to find the <color=#FF1919FF>Impostors</color>", "Hack to find the Impostors", RoleId.Hacker);

        public static readonly RoleInfo NiceMini = new("Nice Mini", Mini.Instance.color,
            "No one will harm you until you grow up", "No one will harm you", RoleId.Mini);

        public static readonly RoleInfo EvilMini = new("Evil Mini", Palette.ImpostorRed,
            "No one will harm you until you grow up", "No one will harm you", RoleId.Mini);

        public static readonly RoleInfo Tracker = new("Tracker", Roles.Tracker.Instance.color,
            "Track the <color=#FF1919FF>Impostors</color> down", "Track the Impostors down", RoleId.Tracker);

        public static readonly RoleInfo Snitch = new("Snitch", Roles.Snitch.Instance.color,
            "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch);

        public static readonly RoleInfo Jackal = new("Jackal", Roles.Jackal.Instance.color,
            "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Jackal);

        public static readonly RoleInfo Sidekick = new("Sidekick", Roles.Sidekick.Instance.color,
            "Help your Jackal to kill everyone",
            "Help your Jackal to kill everyone", RoleId.Sidekick);

        public static readonly RoleInfo Spy = new("Spy", Roles.Spy.Instance.color,
            "Confuse the <color=#FF1919FF>Impostors</color>",
            "Confuse the Impostors", RoleId.Spy);

        public static readonly RoleInfo SecurityGuard = new("Security Guard", Roles.SecurityGuard.Instance.color,
            "Seal vents and place cameras", "Seal vents and place cameras", RoleId.SecurityGuard);

        public static readonly RoleInfo Arsonist =
            new("Arsonist", Roles.Arsonist.Instance.color, "Let them burn", "Let them burn", RoleId.Arsonist);

        public static readonly RoleInfo GoodGuesser = new("Nice Guesser", Guesser.Instance.color, "Guess and shoot",
            "Guess and shoot", RoleId.Guesser);

        public static readonly RoleInfo BadGuesser = new("Evil Guesser", Palette.ImpostorRed,
            "Guess and shoot",
            "Guess and shoot", RoleId.Guesser);

        public static readonly RoleInfo Bait = new("Bait", Roles.Bait.Instance.color, "Bait your enemies",
            "Bait your enemies",
            RoleId.Bait);

        public static readonly RoleInfo Impostor = new("Impostor", Palette.ImpostorRed,
            Helpers.Cs(Palette.ImpostorRed, "Sabotage and kill everyone"), "Sabotage and kill everyone",
            RoleId.Impostor);

        public static readonly RoleInfo Crewmate = new("Crewmate", Color.white, "Find the Impostors",
            "Find the Impostors", RoleId.Crewmate);

        public static readonly RoleInfo Lover =
            new("Lover", Lovers.Instance.color, "You are in love", "You are in love", RoleId.Lover);

        public static readonly List<RoleInfo> AllRoleInfos = new()
        {
            Impostor,
            Godfather,
            Mafioso,
            Janitor,
            Morphling,
            Camouflager,
            Vampire,
            Eraser,
            Trickster,
            Cleaner,
            Warlock,
            BountyHunter,
            NiceMini,
            EvilMini,
            GoodGuesser,
            BadGuesser,
            Lover,
            Jester,
            Arsonist,
            Jackal,
            Sidekick,
            Crewmate,
            Shifter,
            Mayor,
            Engineer,
            Sheriff,
            Lighter,
            Detective,
            TimeMaster,
            Medic,
            Swapper,
            Seer,
            Hacker,
            Tracker,
            Snitch,
            Spy,
            SecurityGuard,
            BountyHunter,
            Bait
        };

        public readonly string introDescription;
        public readonly string name;
        public readonly RoleId roleId;
        public readonly string shortDescription;

        public Color color;

        private RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId)
        {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
        }

        public static List<RoleInfo> GetRoleInfoForPlayer(PlayerControl p)
        {
            var infos = new List<RoleInfo>();
            if (!p) return infos;

            if (Roles.Jackal.FormerJackals.Any(x => x.PlayerId == p.PlayerId))
                infos.Add(Jackal);

            foreach (var (key, _) in RoleReloader.AllRoles.Where(role => role.Value.player == p))
                switch (key)
                {
                    case RoleId.Lover:
                        infos.Add(Lover);
                        break;
                    case RoleId.Mini:
                        infos.Add(p.Data.IsImpostor ? EvilMini : NiceMini);
                        break;
                    case RoleId.Guesser:
                        infos.Add(p.Data.IsImpostor ? BadGuesser : GoodGuesser);
                        break;
                    case RoleId.Jackal:
                        if (!infos.Contains(Jackal))
                            infos.Add(Jackal);
                        break;
                    default:
                        infos.Add(AllRoleInfos.FirstOrDefault(x => x.roleId == key));
                        break;
                }

            // Default roles
            if (infos.Count == 0 && p.Data.IsImpostor) infos.Add(Impostor); // Just Impostor
            if (infos.Count == 0 && !p.Data.IsImpostor) infos.Add(Crewmate); // Just Crewmate

            return infos;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles
{
    public class DeadPlayer
    {
        public readonly PlayerControl killerIfExisting;
        public readonly PlayerControl player;
        public DeathReason deathReason;
        public DateTime timeOfDeath;

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason,
            PlayerControl killerIfExisting)
        {
            this.player = player;
            this.timeOfDeath = timeOfDeath;
            this.deathReason = deathReason;
            this.killerIfExisting = killerIfExisting;
        }
    }

    internal static class GameHistory
    {
        public static List<Tuple<Vector3, bool>> localPlayerPositions = new();
        public static List<DeadPlayer> deadPlayers = new();

        public static void ClearGameHistory()
        {
            localPlayerPositions = new List<Tuple<Vector3, bool>>();
            deadPlayers = new List<DeadPlayer>();
        }
    }
}
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Lovers : Role<Lovers>
    {
        public static bool bothDie = true;

        // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
        public static bool notAckedExiledIsLover;

        // TODO: split lovers into separate roles. this will allow funnies like 3 lovers
        // first player is hidden behind the Role<T>
        public PlayerControl secondPlayer; // this is the second lover

        public override Color color => new Color32(232, 57, 185, byte.MaxValue);
        protected override RoleId roleId => RoleId.Lover;
        public override RoleType roleType => RoleType.Secondary;

        private static bool Existing()
        {
            return Instance.player != null && Instance.secondPlayer != null && !Instance.player.Data.Disconnected &&
                   !Instance.secondPlayer.Data.Disconnected;
        }

        public static bool ExistingAndAlive()
        {
            return Existing() && !Instance.player.Data.IsDead && !Instance.secondPlayer.Data.IsDead &&
                   !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
        }

        public static bool ExistingWithKiller()
        {
            return Existing() &&
                   (Instance.player == Jackal.Instance.player || Instance.secondPlayer == Jackal.Instance.player
                                                              || Instance.player == Sidekick.Instance.player ||
                                                              Instance.secondPlayer == Sidekick.Instance.player
                                                              || Instance.player.Data.IsImpostor ||
                                                              Instance.secondPlayer.Data.IsImpostor);
        }

        public static bool HasAliveKillingLover(PlayerControl player)
        {
            if (!ExistingAndAlive() || !ExistingWithKiller())
                return false;
            return player != null && (player == Instance.player || player == Instance.secondPlayer);
        }

        public static PlayerControl GetPartner(PlayerControl player)
        {
            if (player == null)
                return null;
            if (Instance.player == player)
                return Instance.secondPlayer;
            if (Instance.secondPlayer == player)
                return Instance.player;
            return null;
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            secondPlayer = null;
            notAckedExiledIsLover = false;
            bothDie = CustomOptionHolder.loversBothDie.GetBool();
        }
    }
}
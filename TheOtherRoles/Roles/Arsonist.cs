using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Arsonist : Role<Arsonist>
    {
        public static float cooldown = 30f;
        public static float duration = 3f;
        public static bool triggerArsonistWin;

        public static PlayerControl currentTarget;
        public static PlayerControl douseTarget;
        public static readonly List<PlayerControl> DousedPlayers = new();

        private static Sprite douseSprite;
        private static Sprite igniteSprite;
        public override Color color => new Color32(238, 112, 46, byte.MaxValue);
        protected override RoleId roleId => RoleId.Arsonist;
        public override RoleType roleType => RoleType.Neutral;

        public static Sprite GetDouseSprite()
        {
            if (douseSprite) return douseSprite;
            douseSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.DouseButton.png", 115f);
            return douseSprite;
        }

        public static Sprite GetIgniteSprite()
        {
            if (igniteSprite) return igniteSprite;
            igniteSprite = Helpers.LoadSpriteFromResources("TheOtherRoles.Resources.IgniteButton.png", 115f);
            return igniteSprite;
        }

        public static bool DousedEveryoneAlive()
        {
            return PlayerControl.AllPlayerControls.ToArray().All(x =>
                x == Instance.player || x.Data.IsDead || x.Data.Disconnected ||
                DousedPlayers.Select(y => y.PlayerId).Contains(x.PlayerId));
        }

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            currentTarget = null;
            douseTarget = null;
            triggerArsonistWin = false;
            DousedPlayers.Clear();
            foreach (var p in MapOptions.playerIcons.Values.Where(p => p != null && p.gameObject != null))
                p.gameObject.SetActive(false);
            cooldown = CustomOptionHolder.arsonistCooldown.GetFloat();
            duration = CustomOptionHolder.arsonistDuration.GetFloat();
        }
    }
}
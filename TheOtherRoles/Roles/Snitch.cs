using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Roles
{
    public class Snitch : Role<Snitch>
    {
        public static int taskCountForReveal = 1;
        public static bool includeTeamJackal;
        public static bool teamJackalUseDifferentArrowColor = true;

        public static readonly List<Arrow> LocalArrows = new();
        public override Color color => new Color32(184, 251, 79, byte.MaxValue);
        protected override RoleId roleId => RoleId.Snitch;

        public override void ClearAndReload()
        {
            base.ClearAndReload();
            foreach (var arrow in LocalArrows.Select(arrow => arrow?.arrow))
                Object.Destroy(arrow);
            LocalArrows.Clear();
            taskCountForReveal = Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.GetFloat());
            includeTeamJackal = CustomOptionHolder.snitchIncludeTeamJackal.GetBool();
            teamJackalUseDifferentArrowColor = CustomOptionHolder.snitchTeamJackalUseDifferentArrowColor.GetBool();
        }
    }
}
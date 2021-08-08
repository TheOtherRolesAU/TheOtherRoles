using System;
using System.Linq;
using HarmonyLib;
using InnerNet;
using TheOtherRoles.Objects;
using TheOtherRoles.Roles;
using UnityEngine;
using static TheOtherRoles.RoleReloader;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal class HudManagerUpdatePatch
    {
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");

        private static bool HidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (!MapOptions.hidePlayerNames) return false; // All names are visible
            if (source == null || target == null) return true;
            if (source == target) return false; // Player sees his own name
            if (source.Data.IsImpostor && (target.Data.IsImpostor || target == Spy.Instance.player))
                return false; // Members of team Impostors see the names of Impostors/Spies
            if ((source == Lovers.Instance.player || target == Lovers.Instance.secondPlayer) &&
                (source == Lovers.Instance.player || target == Lovers.Instance.secondPlayer))
                return false; // Members of team Lovers see the names of each other
            if ((source == Jackal.Instance.player || source == Sidekick.Instance.player) &&
                (target == Jackal.Instance.player ||
                 target == Sidekick.Instance.player ||
                 target == Jackal.fakeSidekick))
                return false; // Members of team Jackal see the names of each other
            return true;
        }

        private static void ResetNameTagsAndColors()
        {
            var playersById = Helpers.AllPlayersById();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.nameText.text = HidePlayerName(PlayerControl.LocalPlayer, player) ? "" : player.Data.PlayerName;
                if (PlayerControl.LocalPlayer.Data.IsImpostor && player.Data.IsImpostor)
                    player.nameText.color = Palette.ImpostorRed;
                else
                    player.nameText.color = Color.white;
            }

            if (MeetingHud.Instance != null)
                foreach (var player in MeetingHud.Instance.playerStates)
                {
                    var playerControl = playersById.ContainsKey(player.TargetPlayerId)
                        ? playersById[player.TargetPlayerId]
                        : null;
                    if (playerControl == null) continue;
                    player.NameText.text = playerControl.Data.PlayerName;
                    if (PlayerControl.LocalPlayer.Data.IsImpostor && playerControl.Data.IsImpostor)
                        player.NameText.color = Palette.ImpostorRed;
                    else
                        player.NameText.color = Color.white;
                }

            if (!PlayerControl.LocalPlayer.Data.IsImpostor) return;
            var impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
            impostors.RemoveAll(x => !x.Data.IsImpostor);
            foreach (var player in impostors)
                player.nameText.color = Palette.ImpostorRed;
            if (MeetingHud.Instance == null) return;
            foreach (var player in MeetingHud.Instance.playerStates)
            {
                var playerControl = Helpers.PlayerById(player.TargetPlayerId);
                if (playerControl != null && playerControl.Data.IsImpostor)
                    player.NameText.color = Palette.ImpostorRed;
            }
        }

        private static void SetPlayerNameColor(PlayerControl p, Color color)
        {
            p.nameText.color = color;
            if (!MeetingHud.Instance) return;
            foreach (var player in MeetingHud.Instance.playerStates)
                if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                    player.NameText.color = color;
        }

        private static void SetNameColors()
        {
            // TODO: set of visible roles derived from RoleBase


            // Impostors, Crewmates, some Neutrals
            foreach (var (_, role) in AllRoles.Where(role =>
                role.Value.roleType != RoleType.Secondary && role.Value.player != PlayerControl.LocalPlayer))
                SetPlayerNameColor(role.player, role.color);

            // Others
            if (Jackal.Instance.player == PlayerControl.LocalPlayer)
            {
                // Jackal can see his sidekick
                if (Sidekick.Instance.player != null)
                    SetPlayerNameColor(Sidekick.Instance.player, Jackal.Instance.color);
                if (Jackal.fakeSidekick != null)
                    SetPlayerNameColor(Jackal.fakeSidekick, Jackal.Instance.color);
            }
            else if (Guesser.Instance.player == PlayerControl.LocalPlayer)
            {
                // Guesser can be an Imp
                if (Guesser.Instance.player.Data.IsImpostor)
                    SetPlayerNameColor(Guesser.Instance.player, Palette.ImpostorRed);
            }
            // No else if here, as a Lover of team Jackal needs the colors
            else if (Sidekick.Instance.player != null && Sidekick.Instance.player == PlayerControl.LocalPlayer)
            {
                // Sidekick can see the jackal
                if (Jackal.Instance.player != null)
                    SetPlayerNameColor(Jackal.Instance.player, Jackal.Instance.color);
            }

            // Additional paints
            if (Spy.Instance.player != null && PlayerControl.LocalPlayer.Data.IsImpostor)
                SetPlayerNameColor(Spy.Instance.player, Spy.Instance.color);
        }

        private static void SetNameTags()
        {
            // Mafia
            if (PlayerControl.LocalPlayer.Data.IsImpostor)
            {
                if (Godfather.Instance.player)
                    Godfather.Instance.player.nameText.text = Godfather.Instance.player.Data.PlayerName + " (G)";
                if (Mafioso.Instance.player)
                    Mafioso.Instance.player.nameText.text = Mafioso.Instance.player.Data.PlayerName + " (M)";
                if (Janitor.Instance.player)
                    Janitor.Instance.player.nameText.text = Janitor.Instance.player.Data.PlayerName + " (J)";

                if (MeetingHud.Instance != null)
                    foreach (var player in MeetingHud.Instance.playerStates)
                        if (Godfather.Instance.player.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Godfather.Instance.player.Data.PlayerName + " (G)";
                        else if (Mafioso.Instance.player.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Mafioso.Instance.player.Data.PlayerName + " (M)";
                        else if (Janitor.Instance.player.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Janitor.Instance.player.Data.PlayerName + " (J)";
            }

            // Lovers
            if (Lovers.Instance.player != null && Lovers.Instance.secondPlayer != null &&
                (Lovers.Instance.player == PlayerControl.LocalPlayer ||
                 Lovers.Instance.secondPlayer == PlayerControl.LocalPlayer))
            {
                var suffix = Helpers.Cs(Lovers.Instance.color, " ♥");
                Lovers.Instance.player.nameText.text += suffix;
                Lovers.Instance.secondPlayer.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (var player in MeetingHud.Instance.playerStates)
                        if (Lovers.Instance.player.PlayerId == player.TargetPlayerId ||
                            Lovers.Instance.secondPlayer.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }
        }

        private static void UpdateShielded()
        {
            if (Medic.shielded == null) return;

            if (Medic.shielded.Data.IsDead || Medic.Instance.player == null || Medic.Instance.player.Data.IsDead)
                Medic.shielded = null;
        }

        private static void TimerUpdate()
        {
            Hacker.hackerTimer -= Time.deltaTime;
            Lighter.lighterTimer -= Time.deltaTime;
            Trickster.lightsOutTimer -= Time.deltaTime;
        }

        private static void CamouflageAndMorphActions()
        {
            var oldCamouflageTimer = Camouflager.camouflageTimer;
            var oldMorphTimer = Morphling.morphTimer;

            Camouflager.camouflageTimer -= Time.deltaTime;
            Morphling.morphTimer -= Time.deltaTime;

            // Morphling player size is not here

            // Set morphling morphed look
            if (Morphling.morphTimer > 0f && Camouflager.camouflageTimer <= 0f)
                if (Morphling.Instance.player != null && Morphling.morphTarget != null)
                {
                    Morphling.Instance.player.nameText.text =
                        HidePlayerName(PlayerControl.LocalPlayer, Morphling.Instance.player)
                            ? ""
                            : Morphling.morphTarget.Data.PlayerName;
                    Morphling.Instance.player.myRend.material.SetColor(BackColor,
                        Palette.ShadowColors[Morphling.morphTarget.Data.ColorId]);
                    Morphling.Instance.player.myRend.material.SetColor(BodyColor,
                        Palette.PlayerColors[Morphling.morphTarget.Data.ColorId]);
                    Morphling.Instance.player.HatRenderer.SetHat(Morphling.morphTarget.Data.HatId,
                        Morphling.morphTarget.Data.ColorId);
                    Morphling.Instance.player.nameText.transform.localPosition = new Vector3(0f,
                        (Morphling.morphTarget.Data.HatId == 0U ? 0.7f : 1.05f) * 2f, -0.5f);

                    if (Morphling.Instance.player.MyPhysics.Skin.skin.ProdId != DestroyableSingleton<HatManager>
                        .Instance
                        .AllSkins.ToArray()[(int) Morphling.morphTarget.Data.SkinId].ProdId)
                        Helpers.SetSkinWithAnim(Morphling.Instance.player.MyPhysics, Morphling.morphTarget.Data.SkinId);
                    if (Morphling.Instance.player.CurrentPet == null || Morphling.Instance.player.CurrentPet.ProdId !=
                        DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[
                                (int) Morphling.morphTarget.Data.PetId]
                            .ProdId)
                    {
                        if (Morphling.Instance.player.CurrentPet)
                            Object.Destroy(Morphling.Instance.player.CurrentPet.gameObject);
                        Morphling.Instance.player.CurrentPet = Object.Instantiate(
                            DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[
                                (int) Morphling.morphTarget.Data.PetId]);
                        Morphling.Instance.player.CurrentPet.transform.position =
                            Morphling.Instance.player.transform.position;
                        Morphling.Instance.player.CurrentPet.Source = Morphling.Instance.player;
                        Morphling.Instance.player.CurrentPet.Visible = Morphling.Instance.player.Visible;
                        PlayerControl.SetPlayerMaterialColors(Morphling.morphTarget.Data.ColorId,
                            Morphling.Instance.player.CurrentPet.rend);
                    }
                    else if (Morphling.Instance.player.CurrentPet)
                    {
                        PlayerControl.SetPlayerMaterialColors(Morphling.morphTarget.Data.ColorId,
                            Morphling.Instance.player.CurrentPet.rend);
                    }
                }

            // Set camouflaged look (overrides morphling morphed look if existent)
            if (Camouflager.camouflageTimer > 0f)
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    p.nameText.text = "";
                    p.myRend.material.SetColor(BackColor, Palette.PlayerColors[6]);
                    p.myRend.material.SetColor(BodyColor, Palette.PlayerColors[6]);
                    p.HatRenderer.SetHat(0, 0);
                    Helpers.SetSkinWithAnim(p.MyPhysics, 0);
                    var spawnPet = false;
                    if (p.CurrentPet == null)
                    {
                        spawnPet = true;
                    }
                    else if (p.CurrentPet.ProdId !=
                             DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[0].ProdId)
                    {
                        Object.Destroy(p.CurrentPet.gameObject);
                        spawnPet = true;
                    }

                    if (!spawnPet) continue;
                    p.CurrentPet =
                        Object.Instantiate(DestroyableSingleton<HatManager>.Instance.AllPets.ToArray()[0]);
                    p.CurrentPet.transform.position = p.transform.position;
                    p.CurrentPet.Source = p;
                }

            // Everyone but morphling reset
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f) Camouflager.ResetCamouflage();

            // Morphling reset
            if ((oldMorphTimer > 0f || oldCamouflageTimer > 0f) && Camouflager.camouflageTimer <= 0f &&
                Morphling.morphTimer <= 0f && Morphling.Instance.player != null) Morphling.ResetMorph();
        }

        private static void MiniUpdate()
        {
            if (Mini.Instance.player == null || Camouflager.camouflageTimer > 0f) return;

            var growingProgress = Mini.GrowingProgress();
            var suffix = "";
            if (Math.Abs(growingProgress - 1f) > 0.1f)
                suffix = Helpers.Cs(new Color32(250, 217, 52, byte.MaxValue),
                    $"({Mathf.FloorToInt(growingProgress * 18)}");

            Mini.Instance.player.nameText.text += suffix;
            if (MeetingHud.Instance != null)
                foreach (var player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.Instance.player.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;

            if (Morphling.Instance.player != null && Morphling.morphTarget == Mini.Instance.player &&
                Morphling.morphTimer > 0f)
                Morphling.Instance.player.nameText.text += suffix;
        }

        private static void UpdateImpostorKillButton(HudManager __instance)
        {
            if (!PlayerControl.LocalPlayer.Data.IsImpostor) return;
            var enabled = true;
            if (Vampire.Instance.player != null && Vampire.Instance.player == PlayerControl.LocalPlayer)
                enabled = false;
            else if (Mafioso.Instance.player != null && Mafioso.Instance.player == PlayerControl.LocalPlayer &&
                     Godfather.Instance.player != null && !Godfather.Instance.player.Data.IsDead)
                enabled = false;
            else if (Janitor.Instance.player != null && Janitor.Instance.player == PlayerControl.LocalPlayer)
                enabled = false;
            enabled &= __instance.UseButton.isActiveAndEnabled;

            __instance.KillButton.gameObject.SetActive(enabled);
            __instance.KillButton.renderer.enabled = enabled;
            __instance.KillButton.isActive = enabled;
            __instance.KillButton.enabled = enabled;
        }

        private static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

            CustomButton.HudUpdate();
            ResetNameTagsAndColors();
            SetNameColors();
            UpdateShielded();
            SetNameTags();

            // Impostors
            UpdateImpostorKillButton(__instance);
            // Timer updates
            TimerUpdate();
            // Camouflager and Morphling
            CamouflageAndMorphActions();
            // Mini
            MiniUpdate();
        }
    }
}
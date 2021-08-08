using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using TMPro;
using Twitch;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Modules
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class ModUpdaterButton
    {
        private static void Prefix(MainMenuManager __instance)
        {
            CustomHatLoader.LaunchHatFetcher();
            ModUpdater.LaunchUpdater();
            if (!ModUpdater.hasUpdate) return;
            var template = GameObject.Find("ExitGameButton");
            if (template == null) return;

            var button = Object.Instantiate(template, null);
            var pos = button.transform.localPosition;
            pos.y += 0.6f;
            button.transform.localPosition = pos;

            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((UnityAction) OnClick);

            var text = button.transform.GetChild(0).GetComponent<TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f,
                new Action<float>(_ => { text.SetText("Update\nThe Other Roles"); })));

            var man = DestroyableSingleton<TwitchManager>.Instance;
            ModUpdater.infoPopup = Object.Instantiate(man.TwitchPopup);
            ModUpdater.infoPopup.TextAreaTMP.fontSize *= 0.7f;
            ModUpdater.infoPopup.TextAreaTMP.enableAutoSizing = false;

            void OnClick()
            {
                ModUpdater.ExecuteUpdate();
                button.SetActive(false);
            }
        }
    }

    public static class ModUpdater
    {
        private static bool running;
        public static bool hasUpdate;
        private static string updateUri;
        private static Task updateTask;
        public static GenericPopup infoPopup;

        public static void LaunchUpdater()
        {
            if (running) return;
            running = true;
            CheckForUpdate().GetAwaiter().GetResult();
            ClearOldVersions();
        }

        public static void ExecuteUpdate()
        {
            var info = "Updating The Other Roles\nPlease wait...";
            infoPopup.Show(info); // Show originally
            if (updateTask == null)
            {
                if (updateUri != null)
                    updateTask = DownloadUpdate();
                else
                    info = "Unable to auto-update\nPlease update manually";
            }
            else
            {
                info = "Update might already\nbe in progress";
            }

            infoPopup.StartCoroutine(Effects.Lerp(0.01f, new Action<float>(_ => { SetPopupText(info); })));
        }

        private static void ClearOldVersions()
        {
            try
            {
                var d = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins");
                var files = d.GetFiles("*.old").Select(x => x.FullName).ToArray(); // Getting old versions
                foreach (var f in files)
                    File.Delete(f);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception occured when clearing old versions:\n" + e);
            }
        }

        private static async Task<bool> CheckForUpdate()
        {
            try
            {
                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");
                var response =
                    await http.GetAsync(new Uri("https://api.github.com/repos/Eisbison/TheOtherRoles/releases/latest"),
                        HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode);
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                var tagName = data["tag_name"]?.ToString();
                if (tagName == null) return false; // Something went wrong
                // check version
                var ver = Version.Parse(tagName.Replace("v", ""));
                var diff = TheOtherRolesPlugin.Version.CompareTo(ver);
                if (diff < 0)
                {
                    // Update required
                    hasUpdate = true;
                    var assets = data["assets"];
                    if (!assets.HasValues)
                        return false;

                    for (var current = assets.First; current != null; current = current.Next)
                    {
                        var browserDownloadURL = current["browser_download_url"]?.ToString();
                        if (browserDownloadURL == null || current["content_type"] == null) continue;
                        if (!current["content_type"].ToString().Equals("application/x-msdownload") ||
                            !browserDownloadURL.EndsWith(".dll")) continue;
                        updateUri = browserDownloadURL;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                TheOtherRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }

            return false;
        }

        private static async Task<bool> DownloadUpdate()
        {
            try
            {
                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");
                var response = await http.GetAsync(new Uri(updateUri), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode);
                    return false;
                }

                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var fullname = Uri.UnescapeDataString(uri.Path);
                if (File.Exists(fullname + ".old")) // Clear old file in case it wasn't;
                    File.Delete(fullname + ".old");

                File.Move(fullname, fullname + ".old"); // rename current executable to old

                await using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    await using (var fileStream = File.Create(fullname))
                    {
                        // probably want to have proper name here
                        await responseStream.CopyToAsync(fileStream);
                    }
                }

                ShowPopup("The Other Roles\nupdated successfully\nPlease restart the game.");
                return true;
            }
            catch (Exception ex)
            {
                TheOtherRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }

            ShowPopup("Update wasn't successful\nTry again later,\nor update manually.");
            return false;
        }

        private static void ShowPopup(string message)
        {
            SetPopupText(message);
            infoPopup.gameObject.SetActive(true);
        }

        private static void SetPopupText(string message)
        {
            if (infoPopup == null)
                return;
            if (infoPopup.TextAreaTMP != null) infoPopup.TextAreaTMP.text = message;
        }
    }
}
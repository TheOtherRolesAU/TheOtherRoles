using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Il2CppSystem;
using Hazel;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnhollowerBaseLib;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Twitch;

namespace TheOtherRoles.Modules {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class ModUpdaterButton {
        private static void Prefix(MainMenuManager __instance) {
            CustomHatLoader.LaunchHatFetcher();
            ModUpdater.LaunchUpdater();
            if (!ModUpdater.hasUpdate) return;
            var template = GameObject.Find("ExitGameButton");
            if (template == null) return;

            var button = UnityEngine.Object.Instantiate(template, null);
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 1.2f, button.transform.localPosition.z);

            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            SpriteRenderer buttonSprite = button.GetComponent<SpriteRenderer>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

            var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                text.SetText("Update");
            })));

            buttonSprite.color = text.color = Color.red;
            passiveButton.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate {
                buttonSprite.color = text.color = Color.red;
            });

            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            ModUpdater.InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
            ModUpdater.InfoPopup.TextAreaTMP.fontSize *= 0.7f;
            ModUpdater.InfoPopup.TextAreaTMP.enableAutoSizing = false;

            void onClick() {
                ModUpdater.ExecuteUpdate();
                button.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.UpdateAnnounceText))]
    public static class Announcement {
        public static bool Prefix(AnnouncementPopUp __instance) {       
            var text = __instance.AnnounceTextMeshPro;
            text.text = ModUpdater.announcement;
            return false;
        }
    }

    public class ModUpdater { 
        public static bool running = false;
        public static bool hasUpdate = false;
        public static string updateURI = null;
        private static Task updateTask = null;
        public static string announcement = "";
        public static GenericPopup InfoPopup;

        public static void LaunchUpdater() {
            if (running) return;
            running = true;
            checkForUpdate().GetAwaiter().GetResult();
            clearOldVersions();
            if (hasUpdate || TheOtherRolesPlugin.ShowPopUpVersion.Value != TheOtherRolesPlugin.VersionString) {
                DestroyableSingleton<MainMenuManager>.Instance.Announcement.gameObject.SetActive(true);
                TheOtherRolesPlugin.ShowPopUpVersion.Value = TheOtherRolesPlugin.VersionString;
            }
            MapOptions.reloadPluginOptions();
        }

        public static void ExecuteUpdate() {
            string info = "Updating The Other Roles\nPlease wait...";
            ModUpdater.InfoPopup.Show(info); // Show originally
            if (updateTask == null) {
                if (updateURI != null) {
                    updateTask = downloadUpdate();
                } else {
                    info = "Unable to auto-update\nPlease update manually";
                }
            } else {
                info = "Update might already\nbe in progress";
            }
            ModUpdater.InfoPopup.StartCoroutine(Effects.Lerp(0.01f, new System.Action<float>((p) => { ModUpdater.setPopupText(info); })));
        }
        
        public static void clearOldVersions() {
            try {
                DirectoryInfo d = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins");
                string[] files = d.GetFiles("*.old").Select(x => x.FullName).ToArray(); // Getting old versions
                foreach (string f in files)
                    File.Delete(f);
            } catch (System.Exception e) {
                System.Console.WriteLine("Exception occured when clearing old versions:\n" + e);
            }
        }

        public static async Task<bool> checkForUpdate() {
            try {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");
                var response = await http.GetAsync(new System.Uri("https://api.github.com/repos/Eisbison/TheOtherRoles/releases/latest"), HttpCompletionOption.ResponseContentRead);
                // var response = await http.GetAsync(new System.Uri("https://api.github.com/repos/EoF-1141/TheOtherRoles/releases/latest"), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null) {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string json = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);
                
                string tagname = data["tag_name"]?.ToString();
                if (tagname == null) {
                    return false; // Something went wrong
                }

                string changeLog = data["body"]?.ToString();
                if (changeLog != null) announcement = changeLog;
                // check version
                System.Version ver = System.Version.Parse(tagname.Replace("v", ""));
                int diff = TheOtherRolesPlugin.Version.CompareTo(ver);
                if (diff < 0) { // Update required
                    hasUpdate = true;
                    announcement = $@"<size=150%>A new <color=#FC0303>THE OTHER ROLES</color>
update to v{ver} is available</size>

{announcement}";

                    JToken assets = data["assets"];
                    if (!assets.HasValues)
                        return false;

                    for (JToken current = assets.First; current != null; current = current.Next) {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url != null && current["content_type"] != null) {
                            if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                browser_download_url.EndsWith(".dll")) {
                                updateURI = browser_download_url;
                                return true;
                            }
                        }
                    }
                }  else {
                    announcement = $@"<size=150%><color=#FC0303>THE OTHER ROLES</color> Version {ver}:</size>

{announcement}";
                }
            } catch (System.Exception ex) {
                TheOtherRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }
            return false;
        }

        public static async Task<bool> downloadUpdate() {
            try {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");
                var response = await http.GetAsync(new System.Uri(updateURI), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null) {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                string fullname = System.Uri.UnescapeDataString(uri.Path);
                if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                    File.Delete(fullname + ".old");

                File.Move(fullname, fullname + ".old"); // rename current executable to old

                using (var responseStream = await response.Content.ReadAsStreamAsync()) {
                    using (var fileStream = File.Create(fullname)) { // probably want to have proper name here
                        responseStream.CopyTo(fileStream); 
                    }
                }
                showPopup("The Other Roles\nupdated successfully\nPlease restart the game.");
                return true;
            } catch (System.Exception ex) {
                TheOtherRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }
            showPopup("Update wasn't successful\nTry again later,\nor update manually.");
            return false;
        }
        private static void showPopup(string message) {
            setPopupText(message);
            InfoPopup.gameObject.SetActive(true);
        }

        public static void setPopupText(string message) {
            if (InfoPopup == null)
                return;
            if (InfoPopup.TextAreaTMP != null) {
                InfoPopup.TextAreaTMP.text = message;
            }
        }
    }
}
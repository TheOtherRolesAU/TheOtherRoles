using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AmongUs.Data;
using Assets.InnerNet;
using Twitch;
using static StarGen;

namespace TheOtherRoles.Modules {
    public class ModUpdater : MonoBehaviour {
        public const string RepositoryOwner = "TheOtherRolesAU";
        public const string RepositoryName = "TheOtherRoles";
        public static ModUpdater Instance { get; private set; }

        public ModUpdater(IntPtr ptr) : base(ptr) { }

        private bool _busy;
        private bool showPopUp = true;
        public List<GithubRelease> Releases;

        public void Awake() {
            if (Instance) Destroy(Instance);
            Instance = this;
            foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old")) {
                File.Delete(file);
            }
        }

        private void Start() {
            if (_busy) return;
            this.StartCoroutine(CoCheckForUpdate());
            SceneManager.add_sceneLoaded((System.Action<Scene, LoadSceneMode>)(OnSceneLoaded));
        }
        
            
        [HideFromIl2Cpp]
        public void StartDownloadRelease(GithubRelease release) {
            if (_busy) return;
            this.StartCoroutine(CoDownloadRelease(release));
        }

        [HideFromIl2Cpp]
        private IEnumerator CoCheckForUpdate() {
            _busy = true;
            var www = new UnityWebRequest();
            www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
            www.SetUrl($"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases");
            www.downloadHandler = new DownloadHandlerBuffer();
            var operation = www.SendWebRequest();

            while (!operation.isDone) {
                yield return new WaitForEndOfFrame();
            }

            if (www.isNetworkError || www.isHttpError) {
                yield break;
            }

            Releases = JsonSerializer.Deserialize<List<GithubRelease>>(www.downloadHandler.text);
            www.downloadHandler.Dispose();
            www.Dispose();
            Releases.Sort(SortReleases);
            _busy = false;
        }

        [HideFromIl2Cpp]
        private IEnumerator CoDownloadRelease(GithubRelease release) {
            _busy = true;

            var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
            popup.TextAreaTMP.fontSize *= 0.7f;
            popup.TextAreaTMP.enableAutoSizing = false;

            popup.Show();

            var button = popup.transform.GetChild(2).gameObject;
            button.SetActive(false);
            popup.TextAreaTMP.text = $"Updating TOR\nPlease wait...";

            var asset = release.Assets.Find(FilterPluginAsset);
            var www = new UnityWebRequest();
            www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
            www.SetUrl(asset.DownloadUrl);
            www.downloadHandler = new DownloadHandlerBuffer();
            var operation = www.SendWebRequest();

            while (!operation.isDone) {
                int stars = Mathf.CeilToInt(www.downloadProgress * 10);
                string progress = $"Updating TOR\nPlease wait...\nDownloading...\n{new String((char)0x25A0, stars) + new String((char)0x25A1, 10 - stars)}";
                popup.TextAreaTMP.text = progress;
                yield return new WaitForEndOfFrame();
            }
            
            if (www.isNetworkError || www.isHttpError) {
                popup.TextAreaTMP.text = "Update wasn't successful\nTry again later,\nor update manually.";
                yield break;
            }
            popup.TextAreaTMP.text = $"Updating TOR\nPlease wait...\n\nDownload complete\ncopying file...";

            var filePath = Path.Combine(Paths.PluginPath, asset.Name);

            if (File.Exists(filePath + ".old")) File.Delete(filePath + "old");
            if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

            var persistTask = File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
            var hasError = false;
            while (!persistTask.IsCompleted) {
                if (persistTask.Exception != null) {
                    hasError = true;
                    break;
                }
                
                yield return new WaitForEndOfFrame();
            }

            www.downloadHandler.Dispose();
            www.Dispose();

            if (!hasError) {
                popup.TextAreaTMP.text = $"TheOtherRoles\nupdated successfully\nPlease restart the game.";
            }
            button.SetActive(true);
            _busy = false;
        }

        [HideFromIl2Cpp]
        private static bool FilterLatestRelease(GithubRelease release) {
            return release.IsNewer(TheOtherRolesPlugin.Version) && release.Assets.Any(FilterPluginAsset);
        }

        [HideFromIl2Cpp]
        private static bool FilterPluginAsset(GithubAsset asset) {
            return asset.Name == "TheOtherRoles.dll";
        }

        [HideFromIl2Cpp]
        private static int SortReleases(GithubRelease a, GithubRelease b) {
            if (a.IsNewer(b.Version)) return -1;
            if (b.IsNewer(a.Version)) return 1;
            return 0;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (_busy || scene.name != "MainMenu") return;
            var latestRelease = Releases.FirstOrDefault();
            if (latestRelease == null || latestRelease.Version <= TheOtherRolesPlugin.Version)
                return;

            var template = GameObject.Find("ExitGameButton");
            if (!template) return;

            var button = Instantiate(template, null);
            var buttonTransform = button.transform;
            //buttonTransform.localPosition = new Vector3(-2f, -2f);
            button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.458f, 0.124f);

            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action)(() =>
            {
                StartDownloadRelease(latestRelease);
                button.SetActive(false);
            }));

            var text = button.transform.GetComponentInChildren<TMPro.TMP_Text>();
            string t = "Update TOR";
            StartCoroutine(Effects.Lerp(0.1f, (System.Action<float>)(p => text.SetText(t))));
            passiveButton.OnMouseOut.AddListener((Action)(() => text.color = Color.red));
            passiveButton.OnMouseOver.AddListener((Action)(() => text.color = Color.white));
            var announcement = $"<size=150%>A new THE OTHER ROLES update to {latestRelease.Tag} is available</size>\n{latestRelease.Description}";
            var mgr = FindObjectOfType<MainMenuManager>(true);
            if (showPopUp) mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: "TOR Update", date : latestRelease.PublishedAt)) ;
            showPopUp = false;

        }

        [HideFromIl2Cpp]
        public IEnumerator CoShowAnnouncement(string announcement, bool show = true, string shortTitle = "TOR Update", string title = "", string date = "") {
            var mgr = FindObjectOfType<MainMenuManager>(true);
            var popUpTemplate = UnityEngine.Object.FindObjectOfType<AnnouncementPopUp>(true);
            if (popUpTemplate == null) {
                TheOtherRolesPlugin.Logger.LogError("couldnt show credits, popUp is null");
                yield return null;
            }
            var popUp = UnityEngine.Object.Instantiate(popUpTemplate);

            popUp.gameObject.SetActive(true);

            Assets.InnerNet.Announcement creditsAnnouncement = new() {
                Id = "torAnnouncement",
                Language = 0,
                Number = 6969,
                Title = title == "" ? "The Other Roles Announcement" : title,
                ShortTitle = shortTitle,
                SubTitle = "",
                PinState = false,
                Date = date == "" ? DateTime.Now.Date.ToString() : date,
                Text = announcement,
            };
            mgr.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                if (p == 1) {
                    var backup = DataManager.Player.Announcements.allAnnouncements;
                    DataManager.Player.Announcements.allAnnouncements = new();
                    popUp.Init(false);
                    DataManager.Player.Announcements.SetAnnouncements(new Announcement[] { creditsAnnouncement });
                    popUp.CreateAnnouncementList();
                    popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                    popUp.visibleAnnouncements[0].PassiveButton.OnClick.RemoveAllListeners();
                    DataManager.Player.Announcements.allAnnouncements = backup;
                }
            })));
        }
    }

    public class GithubRelease {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tag_name")]
        public string Tag { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("published_at")]
        public string PublishedAt { get; set; }

        [JsonPropertyName("body")]
        public string Description { get; set; }

        [JsonPropertyName("assets")]
        public List<GithubAsset> Assets { get; set; }

        public Version Version => Version.Parse(Tag.Replace("v", string.Empty));

        public bool IsNewer(Version version) {
            return Version > version;
        }
    }

    public class GithubAsset {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
}

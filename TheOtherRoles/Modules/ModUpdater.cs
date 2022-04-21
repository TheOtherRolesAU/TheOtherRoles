using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.IL2CPP.Utils;
using Newtonsoft.Json.Linq;
using TMPro;
using Twitch;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Version = SemanticVersioning.Version;

namespace TheOtherRoles.Modules 
{
    public class ModUpdateBehaviour : MonoBehaviour
    {
        public static ModUpdateBehaviour Instance { get; private set; }
        public ModUpdateBehaviour(IntPtr ptr) : base(ptr) { }

        public class UpdateData
        {
            public string Content;
            public string Tag;
            public JObject Request;
            
            public UpdateData(JObject data)
            {
                Tag = data["tag_name"]?.ToString().TrimStart('v');
                Content = data["body"]?.ToString();
                Request = data;
            }

            public bool IsNewer(Version version)
            {
                if (!Version.TryParse(Tag, out var myVersion)) return false;
                return myVersion > version;
            }
        }

        public UpdateData TORUpdate;
        public UpdateData SubmergedUpdate;

        public UpdateData RequiredUpdateData => TORUpdate ?? SubmergedUpdate;
        
        public void Awake()
        {
            if (Instance) Destroy(this);
            Instance = this;
            
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) (OnSceneLoaded));
            this.StartCoroutine(CoCheckUpdates());
            
            foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old"))
            {
                File.Delete(file);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu") return;
            if (RequiredUpdateData is null) return;
            
            var template = GameObject.Find("ExitGameButton");
            if (!template) return;
            
            var button = Instantiate(template, null);
            var buttonTransform = button.transform;
            var pos = buttonTransform.localPosition;
            pos.y += 1.2f;
            buttonTransform.localPosition = pos;

            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            SpriteRenderer buttonSprite = button.GetComponent<SpriteRenderer>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action) (() =>
            {
                this.StartCoroutine(CoUpdate());
                button.SetActive(false);
            }));

            var text = button.transform.GetChild(0).GetComponent<TMP_Text>();
            StartCoroutine(Effects.Lerp(0.1f, (Action<float>)(p => text.SetText("Update"))));

            buttonSprite.color = text.color = Color.red;
            passiveButton.OnMouseOut.AddListener((Action)(() => buttonSprite.color = text.color = Color.red));

            var isSubmerged = TORUpdate == null;
            var announcement = $"<size=150%>A new <color=#FC0303>{(isSubmerged ? "Submerged" : "THE OTHER ROLES")}</color>\nupdate to {(isSubmerged ? SubmergedUpdate.Tag : TORUpdate.Tag)} is available</size>\n{(isSubmerged ? SubmergedUpdate.Content : TORUpdate.Content)}";
            var mgr = FindObjectOfType<MainMenuManager>(true);
            mgr.StartCoroutine(CoShowAnnouncement(announcement));
        }

        
        [HideFromIl2Cpp]
        public IEnumerator CoUpdate()
        {
            var isSubmerged = TORUpdate is null;
            var updateName = (isSubmerged ? "Submerged" : "The Other Roles");
            
            var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
            popup.TextAreaTMP.fontSize *= 0.7f;
            popup.TextAreaTMP.enableAutoSizing = false;
            popup.Show();
            popup.TextAreaTMP.text = $"Updating {updateName}\nPlease wait...";

            var download = Task.Run(DownloadUpdate);
            while (!download.IsCompleted) yield return null;
            popup.TextAreaTMP.text = download.Result ? $"{updateName}\nupdated successfully\nPlease restart the game." : "Update wasn't successful\nTry again later,\nor update manually.";

        }

        [HideFromIl2Cpp]
        public IEnumerator CoShowAnnouncement(string announcement)
        {
            var popUp = Instantiate(FindObjectOfType<AnnouncementPopUp>(true));
            popUp.gameObject.SetActive(true);
            yield return popUp.Init();
            var last = SaveManager.LastAnnouncement;
            last.Id = 1;
            last.Text = announcement;
            SelectableHyperLinkHelper.DestroyGOs(popUp.selectableHyperLinks, name);
            popUp.AnnounceTextMeshPro.text = announcement;
        }

        [HideFromIl2Cpp]
        public IEnumerator CoCheckUpdates()
        {
            var torUpdateCheck = Task.Run(() => GetGithubUpdate("Eisbison", "TheOtherRoles"));
            while (!torUpdateCheck.IsCompleted) yield return null;
            if (torUpdateCheck.Result != null && torUpdateCheck.Result.IsNewer(Version.Parse(TheOtherRolesPlugin.VersionString)))
            {
                TORUpdate = torUpdateCheck.Result;
            }
            
            var submergedUpdateCheck = Task.Run(() => GetGithubUpdate("SubmergedAmongUs", "Submerged"));
            while (!submergedUpdateCheck.IsCompleted) yield return null;
            if (submergedUpdateCheck.Result != null && !Submerged.Debug && (!Submerged.Loaded || submergedUpdateCheck.Result.IsNewer(Submerged.Version)))
            {
                SubmergedUpdate = submergedUpdateCheck.Result;
            }
        }

        [HideFromIl2Cpp]
        public async Task<UpdateData> GetGithubUpdate(string owner, string repo)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");

            var req = await client.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest", HttpCompletionOption.ResponseContentRead);
            if (!req.IsSuccessStatusCode) return null;

            var dataString = await req.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(dataString);
            return new UpdateData(data);
        }

        [HideFromIl2Cpp]
        public async Task<bool> DownloadUpdate()
        {
            var isSubmerged = TORUpdate is null;
            var data = isSubmerged ? SubmergedUpdate : TORUpdate;
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");
            
            JToken assets = data.Request["assets"];
            string downloadURI = "";
            for (JToken current = assets.First; current != null; current = current.Next) 
            {
                string browser_download_url = current["browser_download_url"]?.ToString();
                if (browser_download_url != null && current["content_type"] != null) {
                    if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                        browser_download_url.EndsWith(".dll")) {
                        downloadURI = browser_download_url;
                        break;
                    }
                }
            }

            if (downloadURI.Length == 0) return false;

            var res = await client.GetAsync(downloadURI, HttpCompletionOption.ResponseContentRead);
            string codeBase = (isSubmerged ? Submerged.Assembly : Assembly.GetExecutingAssembly()).CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string fullname = Uri.UnescapeDataString(uri.Path);
            if (File.Exists(fullname + ".old")) File.Delete(fullname + ".old");
            File.Move(fullname, fullname + ".old");

            await using var responseStream = await res.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(fullname);
            await responseStream.CopyToAsync(fileStream);

            return true;
        }
    }
}
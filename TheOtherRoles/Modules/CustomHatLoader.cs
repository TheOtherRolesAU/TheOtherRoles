using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TheOtherRoles.Modules
{
    public static class CustomHatLoader
    {
        private const string Repo = "https://raw.githubusercontent.com/Eisbison/TheOtherHats/master";
        private static bool running;

        public static List<CustomHatOnline> hatdetails = new();
        private static Task hatFetchTask;

        public static void LaunchHatFetcher()
        {
            if (running)
                return;
            running = true;
            hatFetchTask = LaunchHatFetcherAsync();
        }

        private static async Task LaunchHatFetcherAsync()
        {
            try
            {
                var status = await FetchHats();
                if (status != HttpStatusCode.OK)
                    System.Console.WriteLine("Custom Hats could not be loaded\n");
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Unable to fetch hats\n" + e.Message);
            }

            running = false;
        }

        private static string SanitizeResourcePath(string res)
        {
            if (res == null || !res.EndsWith(".png"))
                return null;

            res = res.Replace("\\", "")
                .Replace("/", "")
                .Replace("*", "")
                .Replace("..", "");
            return res;
        }

        private static async Task<HttpStatusCode> FetchHats()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue {NoCache = true};
            var response = await http.GetAsync(new Uri($"{Repo}/CustomHats.json"),
                HttpCompletionOption.ResponseContentRead);
            try
            {
                if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
                if (response.Content == null)
                {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode);
                    return HttpStatusCode.ExpectationFailed;
                }

                var json = await response.Content.ReadAsStringAsync();
                var jobj = JObject.Parse(json)["hats"];
                if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

                var hatdatas = new List<CustomHatOnline>();

                for (var current = jobj.First; current != null; current = current.Next)
                    if (current.HasValues)
                    {
                        var info = new CustomHatOnline
                        {
                            name = current["name"]?.ToString(),
                            resource = SanitizeResourcePath(current["resource"]?.ToString())
                        };

                        if (info.resource == null || info.name == null) // required
                            continue;
                        info.reshasha = current["reshasha"]?.ToString();
                        info.backresource = SanitizeResourcePath(current["backresource"]?.ToString());
                        info.reshashb = current["reshashb"]?.ToString();
                        info.climbresource = SanitizeResourcePath(current["climbresource"]?.ToString());
                        info.reshashc = current["reshashc"]?.ToString();
                        info.flipresource = SanitizeResourcePath(current["flipresource"]?.ToString());
                        info.reshashf = current["reshashf"]?.ToString();
                        info.backflipresource = SanitizeResourcePath(current["backflipresource"]?.ToString());
                        info.reshashbf = current["reshashbf"]?.ToString();

                        info.author = current["author"]?.ToString();
                        info.package = current["package"]?.ToString();
                        info.condition = current["condition"]?.ToString();
                        info.bounce = current["bounce"] != null;
                        info.adaptive = current["adaptive"] != null;
                        info.behind = current["behind"] != null;
                        hatdatas.Add(info);
                    }

                var markedfordownload = new List<string>();

                var filePath = Path.GetDirectoryName(Application.dataPath) + @"\TheOtherHats\";
                var md5 = MD5.Create();
                foreach (var data in hatdatas)
                {
                    if (DoesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                        markedfordownload.Add(data.resource);
                    if (data.backresource != null &&
                        DoesResourceRequireDownload(filePath + data.backresource, data.reshashb, md5))
                        markedfordownload.Add(data.backresource);
                    if (data.climbresource != null &&
                        DoesResourceRequireDownload(filePath + data.climbresource, data.reshashc, md5))
                        markedfordownload.Add(data.climbresource);
                    if (data.flipresource != null &&
                        DoesResourceRequireDownload(filePath + data.flipresource, data.reshashf, md5))
                        markedfordownload.Add(data.flipresource);
                    if (data.backflipresource != null &&
                        DoesResourceRequireDownload(filePath + data.backflipresource, data.reshashbf, md5))
                        markedfordownload.Add(data.backflipresource);
                }

                foreach (var file in markedfordownload)
                {
                    var hatFileResponse =
                        await http.GetAsync($"{Repo}/hats/{file}", HttpCompletionOption.ResponseContentRead);
                    if (hatFileResponse.StatusCode != HttpStatusCode.OK) continue;
                    await using var responseStream = await hatFileResponse.Content.ReadAsStreamAsync();
                    await using var fileStream = File.Create($"{filePath}\\{file}");
                    await responseStream.CopyToAsync(fileStream);
                }

                hatdetails = hatdatas;
            }
            catch (Exception ex)
            {
                TheOtherRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }

            return HttpStatusCode.OK;
        }

        private static bool DoesResourceRequireDownload(string respath, string reshash, MD5 md5)
        {
            if (reshash == null || !File.Exists(respath))
                return true;

            using var stream = File.OpenRead(respath);
            var hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            return !reshash.Equals(hash);
        }

        public class CustomHatOnline : CustomHats.CustomHat
        {
            public string reshasha { get; set; }
            public string reshashb { get; set; }
            public string reshashc { get; set; }
            public string reshashf { get; set; }
            public string reshashbf { get; set; }
        }
    }
}
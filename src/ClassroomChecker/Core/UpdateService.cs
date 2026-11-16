using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ClassroomChecker.Core
{
    /// <summary>ຂໍ້ມູນ release ລ່າສຸດຈາກ GitHub</summary>
    public class ReleaseInfo
    {
        public string Tag;
        public Version Version;
        public string HtmlUrl;
        public string Notes;
        public DateTime? PublishedAt;
        public string ExeUrl;
        public long ExeSize;
        /// <summary>SHA-256 (hex ຕົວພິມນ້ອຍ) ທີ່ GitHub ຄຳນວນໃຫ້ asset; null ຖ້າ API ບໍ່ສົ່ງມາ</summary>
        public string ExeSha256;
        public string ChecksumsUrl;
    }

    /// <summary>
    /// ກວດຫາ ແລະ ຕິດຕັ້ງເວີຊັນໃໝ່ຈາກ GitHub Releases.
    /// ຂັ້ນຕອນ: ຖາມ API → ດາວໂຫຼດ exe ເປັນ .new ຂ້າງ exe ປັດຈຸບັນ → ກວດ SHA-256 →
    /// ປ່ຽນຊື່ exe ທີ່ກຳລັງແລ່ນເປັນ .old → ຍ້າຍ .new ເຂົ້າແທນ → ເປີດໂປຣແກຣມໃໝ່.
    /// ໄຟລ໌ .old ຈະຖືກລຶບຕອນເປີດໂປຣແກຣມເທື່ອຕໍ່ໄປ (CleanupAfterUpdate).
    /// </summary>
    public static class UpdateService
    {
        public const string RepoOwner = "Kittisayst";
        public const string RepoName = "classroom-checker";
        public const string RepoUrl = "https://github.com/" + RepoOwner + "/" + RepoName;
        public const string ReleasesUrl = RepoUrl + "/releases";
        public const string LatestApiUrl = "https://api.github.com/repos/" + RepoOwner + "/" + RepoName + "/releases/latest";
        public const string ExeAssetName = "ClassroomChecker.exe";
        public const string ChecksumsAssetName = "checksums-sha256.txt";

        /// <summary>ດາວໂຫຼດໄດ້ສະເພາະ asset ຂອງ repo ນີ້ເທົ່ານັ້ນ</summary>
        internal const string AllowedDownloadPrefix = RepoUrl + "/releases/download/";

        private const string UserAgent = "ClassroomChecker-Updater";
        private const int TimeoutMs = 15000;

        public static Version CurrentVersion
        {
            get { return Normalize(Assembly.GetExecutingAssembly().GetName().Version); }
        }

        public static string CurrentVersionText
        {
            get { return FormatVersion(CurrentVersion); }
        }

        // ------------------------------------------------------------------ ເວີຊັນ

        /// <summary>"v1.2.0" / "1.2" / "V1.2.0.0" → Version (Build/Revision ທີ່ບໍ່ມີ = 0); ຮູບແບບຜິດ → null</summary>
        public static Version ParseTagVersion(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return null;
            Match m = Regex.Match(tag.Trim(), @"^[vV]?(\d+)\.(\d+)(?:\.(\d+))?(?:\.(\d+))?$");
            if (!m.Success) return null;
            try
            {
                return new Version(
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[2].Value),
                    m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0,
                    m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0);
            }
            catch (OverflowException) { return null; }
        }

        /// <summary>ເຕີມ Build/Revision ທີ່ເປັນ -1 ໃຫ້ເປັນ 0 ເພື່ອທຽບກັນໄດ້ຖືກຕ້ອງ (1.2 == 1.2.0.0)</summary>
        public static Version Normalize(Version v)
        {
            if (v == null) return new Version(0, 0, 0, 0);
            return new Version(v.Major, v.Minor, Math.Max(0, v.Build), Math.Max(0, v.Revision));
        }

        public static bool IsNewer(Version latest, Version current)
        {
            if (latest == null) return false;
            return Normalize(latest).CompareTo(Normalize(current)) > 0;
        }

        /// <summary>1.2.0.0 → "1.2.0"; ສະແດງ Revision ສະເພາະເມື່ອບໍ່ແມ່ນ 0</summary>
        public static string FormatVersion(Version v)
        {
            v = Normalize(v);
            return v.Revision > 0
                ? string.Format("{0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision)
                : string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);
        }

        // ------------------------------------------------------------------ ອ່ານ API

        /// <summary>ແປງ JSON ຂອງ /releases/latest ເປັນ ReleaseInfo; ບໍ່ມີ tag ທີ່ອ່ານໄດ້ → null</summary>
        public static ReleaseInfo ParseRelease(string json)
        {
            var root = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 }.DeserializeObject(json) as IDictionary<string, object>;
            if (root == null) return null;

            var info = new ReleaseInfo
            {
                Tag = GetString(root, "tag_name"),
                HtmlUrl = GetString(root, "html_url"),
                Notes = GetString(root, "body") ?? ""
            };
            info.Version = ParseTagVersion(info.Tag);
            if (info.Version == null) return null;

            DateTime published;
            string pub = GetString(root, "published_at");
            if (!string.IsNullOrEmpty(pub) && DateTime.TryParse(pub, null, System.Globalization.DateTimeStyles.RoundtripKind, out published))
                info.PublishedAt = published.ToLocalTime();

            object assetsObj;
            if (root.TryGetValue("assets", out assetsObj) && assetsObj is IEnumerable)
            {
                foreach (object a in (IEnumerable)assetsObj)
                {
                    var asset = a as IDictionary<string, object>;
                    if (asset == null) continue;
                    string name = GetString(asset, "name");
                    string url = GetString(asset, "browser_download_url");
                    if (string.Equals(name, ExeAssetName, StringComparison.OrdinalIgnoreCase))
                    {
                        info.ExeUrl = url;
                        object size;
                        if (asset.TryGetValue("size", out size) && size != null) info.ExeSize = Convert.ToInt64(size);
                        string digest = GetString(asset, "digest");
                        if (!string.IsNullOrEmpty(digest) && digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                        {
                            string hex = digest.Substring(7).Trim().ToLowerInvariant();
                            if (IsSha256Hex(hex)) info.ExeSha256 = hex;
                        }
                    }
                    else if (string.Equals(name, ChecksumsAssetName, StringComparison.OrdinalIgnoreCase))
                    {
                        info.ChecksumsUrl = url;
                    }
                }
            }
            return info;
        }

        /// <summary>
        /// ຫາ hash ຂອງ fileName ໃນໄຟລ໌ checksum ຮູບແບບ sha256sum ("&lt;hash&gt;  &lt;ຊື່ໄຟລ໌&gt;" ຫຼື "&lt;hash&gt; *&lt;ຊື່ໄຟລ໌&gt;").
        /// ບໍ່ພົບ → null
        /// </summary>
        public static string ParseChecksums(string text, string fileName)
        {
            if (string.IsNullOrEmpty(text)) return null;
            foreach (string raw in text.Split('\n'))
            {
                Match m = Regex.Match(raw.Trim(), @"^([0-9a-fA-F]{64})\s+\*?(.+)$");
                if (!m.Success) continue;
                string name = Path.GetFileName(m.Groups[2].Value.Trim());
                if (string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase))
                    return m.Groups[1].Value.ToLowerInvariant();
            }
            return null;
        }

        /// <summary>ຕັດ markdown ທີ່ອ່ານຍາກໃນ TextBox ອອກ (ຫົວຂໍ້ ##, **ໂຕໜາ**) ແລະ ປ່ຽນ \n ເປັນ \r\n</summary>
        public static string CleanNotes(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes)) return "";
            string s = notes.Replace("\r\n", "\n").Replace("**", "");
            s = Regex.Replace(s, @"(?m)^#{1,6}\s*", "");
            return s.Trim().Replace("\n", "\r\n");
        }

        internal static bool IsAllowedDownloadUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith(AllowedDownloadPrefix, StringComparison.Ordinal);
        }

        private static bool IsSha256Hex(string s)
        {
            return s != null && Regex.IsMatch(s, "^[0-9a-f]{64}$");
        }

        private static string GetString(IDictionary<string, object> d, string key)
        {
            object v;
            return d.TryGetValue(key, out v) && v != null ? v.ToString() : null;
        }

        // ------------------------------------------------------------------ ເຄືອຂ່າຍ

        private static WebClient CreateClient()
        {
            // .NET 4.8 ເທິງ Windows ເກົ່າອາດບໍ່ເປີດ TLS 1.2 ເອງ; GitHub ຕ້ອງການ TLS 1.2 ຂຶ້ນໄປ
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var wc = new TimeoutWebClient(TimeoutMs);
            wc.Headers[HttpRequestHeader.UserAgent] = UserAgent;
            wc.Encoding = Encoding.UTF8;
            return wc;
        }

        /// <summary>ຖາມ GitHub ຫາ release ລ່າສຸດ. ບໍ່ມີເນັດ ຫຼື API ຜິດພາດ → ໂຍນ exception ພ້ອມຂໍ້ຄວາມທີ່ອ່ານໄດ້</summary>
        public static Task<ReleaseInfo> CheckLatestAsync()
        {
            // ໃຊ້ DownloadString ແບບ sync ໃນ Task.Run: Timeout ຂອງ WebRequest ບໍ່ມີຜົນກັບ method ແບບ async
            return Task.Run(() => CheckLatest());
        }

        private static ReleaseInfo CheckLatest()
        {
            string json;
            using (var wc = CreateClient())
            {
                wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                json = wc.DownloadString(LatestApiUrl);
            }
            ReleaseInfo info = ParseRelease(json);
            if (info == null) throw new InvalidDataException("Release tag is not a version number");

            // API ບາງຄັ້ງບໍ່ສົ່ງ digest: ໃຊ້ໄຟລ໌ checksums-sha256.txt ແທນ
            if (info.ExeSha256 == null && IsAllowedDownloadUrl(info.ChecksumsUrl))
            {
                try
                {
                    using (var wc = CreateClient())
                        info.ExeSha256 = ParseChecksums(wc.DownloadString(info.ChecksumsUrl), ExeAssetName);
                }
                catch (Exception ex) { ReportService.LogError("UpdateService.CheckLatest", ex); }
            }
            return info;
        }

        // ------------------------------------------------------------------ ຕິດຕັ້ງ

        public static string CurrentExePath
        {
            get { return Process.GetCurrentProcess().MainModule.FileName; }
        }

        /// <summary>
        /// ດາວໂຫຼດ exe ໃໝ່ເປັນ "&lt;exe&gt;.new" ໃນໂຟນເດີດຽວກັບ exe ປັດຈຸບັນ ແລະ ກວດ SHA-256.
        /// ກວດບໍ່ຜ່ານ ຫຼື ບໍ່ມີ hash ໃຫ້ກວດ → ລຶບໄຟລ໌ ແລະ ໂຍນ exception (ບໍ່ຕິດຕັ້ງໄຟລ໌ທີ່ບໍ່ໄດ້ກວດ)
        /// </summary>
        public static async Task<string> DownloadAsync(ReleaseInfo release, IProgress<int> progress, CancellationToken token)
        {
            if (release == null) throw new ArgumentNullException("release");
            if (!IsAllowedDownloadUrl(release.ExeUrl))
                throw new InvalidOperationException("Release has no " + ExeAssetName + " from " + RepoUrl);
            if (release.ExeSha256 == null)
                throw new InvalidDataException("Release has no SHA-256 checksum for " + ExeAssetName);

            string target = CurrentExePath + ".new";
            if (File.Exists(target)) File.Delete(target);

            using (var wc = CreateClient())
            using (token.Register(wc.CancelAsync))
            {
                if (progress != null)
                    wc.DownloadProgressChanged += (s, e) => progress.Report(e.ProgressPercentage);
                try
                {
                    await wc.DownloadFileTaskAsync(new Uri(release.ExeUrl), target).ConfigureAwait(false);
                }
                catch (WebException) when (token.IsCancellationRequested)
                {
                    TryDelete(target);
                    throw new OperationCanceledException(token);
                }
            }

            string error = VerifyDownloadedExe(target, release.ExeSha256);
            if (error != null)
            {
                TryDelete(target);
                throw new InvalidDataException(error);
            }
            return target;
        }

        /// <summary>ກວດໄຟລ໌ທີ່ດາວໂຫຼດ: ເປັນໂປຣແກຣມ Windows (MZ) ແລະ SHA-256 ຕົງກັນ. ຜ່ານ → null, ບໍ່ຜ່ານ → ເຫດຜົນ</summary>
        public static string VerifyDownloadedExe(string path, string expectedSha256)
        {
            if (!File.Exists(path)) return "Downloaded file is missing";
            using (var fs = File.OpenRead(path))
            {
                if (fs.Length < 2 || fs.ReadByte() != 'M' || fs.ReadByte() != 'Z')
                    return "Downloaded file is not a Windows program";
                fs.Position = 0;
                string actual = ComputeSha256(fs);
                if (!string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase))
                    return "SHA-256 mismatch (expected " + expectedSha256 + ", got " + actual + ")";
            }
            return null;
        }

        public static string ComputeSha256(Stream s)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(s);
                var sb = new StringBuilder(64);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// ສັບປ່ຽນ exe: current → current.old, newExe → current. ລົ້ມເຫຼວກາງທາງ → ຍ້າຍກັບຄືນ ແລະ ໂຍນ exception.
        /// Windows ອະນຸຍາດໃຫ້ປ່ຽນຊື່ exe ທີ່ກຳລັງແລ່ນໄດ້ ແຕ່ບໍ່ໃຫ້ຂຽນທັບ
        /// </summary>
        public static void SwapExecutable(string currentExe, string newExe)
        {
            string old = currentExe + ".old";
            if (File.Exists(old)) File.Delete(old);

            File.Move(currentExe, old);
            try
            {
                File.Move(newExe, currentExe);
            }
            catch
            {
                try { File.Move(old, currentExe); }
                catch (Exception rollbackEx) { ReportService.LogError("UpdateService.SwapExecutable", rollbackEx); }
                throw;
            }
        }

        /// <summary>ສັບປ່ຽນ exe ແລະ ເປີດເວີຊັນໃໝ່ (ຜູ້ເອີ້ນຕ້ອງປິດໂປຣແກຣມປັດຈຸບັນຕໍ່)</summary>
        public static void InstallAndRestart(string downloadedExe)
        {
            string current = CurrentExePath;
            SwapExecutable(current, downloadedExe);
            ReportService.Log("Update: installed " + Path.GetFileName(current) + " (was " + CurrentVersionText + ")");
            Process.Start(new ProcessStartInfo(current) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(current) });
        }

        /// <summary>
        /// ເອີ້ນຕອນເປີດໂປຣແກຣມ: ລຶບ .old ທີ່ເຫຼືອຈາກການອັບເດດ ແລະ .new ທີ່ດາວໂຫຼດບໍ່ສຳເລັດ.
        /// exe ເກົ່າອາດຍັງປິດບໍ່ທັນ ຈຶ່ງລອງຊ້ຳສອງສາມເທື່ອໃນ background
        /// </summary>
        public static void CleanupAfterUpdate()
        {
            string current;
            try { current = CurrentExePath; }
            catch { return; }

            Task.Run(() =>
            {
                foreach (string leftover in new[] { current + ".old", current + ".new" })
                {
                    for (int attempt = 0; attempt < 5 && File.Exists(leftover); attempt++)
                    {
                        try { File.Delete(leftover); }
                        catch (IOException) { Thread.Sleep(1000); }
                        catch (UnauthorizedAccessException) { Thread.Sleep(1000); }
                    }
                }
            });
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { ReportService.LogError("UpdateService.TryDelete", ex); }
        }

        /// <summary>WebClient ທີ່ມີ timeout (ຄ່າເລີ່ມຕົ້ນ 100 ວິນາທີ ດົນເກີນໄປສຳລັບເນັດຫ້ອງຮຽນທີ່ຫຼຸດ)</summary>
        private sealed class TimeoutWebClient : WebClient
        {
            private readonly int _timeoutMs;
            public TimeoutWebClient(int timeoutMs) { _timeoutMs = timeoutMs; }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest req = base.GetWebRequest(address);
                if (req != null) req.Timeout = _timeoutMs;
                var http = req as HttpWebRequest;
                if (http != null) http.ReadWriteTimeout = _timeoutMs;
                return req;
            }
        }
    }
}

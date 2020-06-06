using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ApkManager.Lib
{
    class Aapt
    {
        public class Result
        {
            public Apk Apk { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        private static BitmapImage GetIconFrom(string pathApk, string pathIcon)
        {
            try
            {
                using (var stream = new MemoryStream())
                using (var zip = ZipStorer.Open(pathApk, FileAccess.Read))
                {
                    if (!pathIcon.EndsWith(".xml"))
                    {
                        var fileEntry = zip.ReadCentralDir().Where(f => f.FilenameInZip.Equals(pathIcon)).SingleOrDefault();
                        zip.ExtractFile(fileEntry, stream);
                    }
                    else
                    {
                        var icon = Path.GetFileNameWithoutExtension(pathIcon) + ".png";
                        var fileEntry = zip.ReadCentralDir().Where(f => f.FilenameInZip.EndsWith(icon) && f.FilenameInZip.Contains("hdpi")).LastOrDefault();
                        zip.ExtractFile(fileEntry, stream);
                    }

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
            }
            catch (Exception e)
            {
                Debug.Print("Apk.GetIcon: {0}", e.Message);
                return App.GetPlaystoreImageFromResources();
            }
        }

        public static async Task<string> RunAsync(string command, params object[] args)
        {
            return await Task.Run(() =>
            {
                using (var p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = Path.Combine("Lib", "aapt.exe"),
                        Arguments = string.Format(command, args)
                    };

                    p.Start();

                    Debug.Print("Aapt.Command: aapt2 {0}", string.Format(command, args));

                    var output = p.StandardOutput.ReadToEnd();
                    var error = p.StandardError.ReadToEnd();

                    p.WaitForExit();
                    
                    if (p.ExitCode != 0) throw new Exception(error);
                    return output;
                }
            });
        }

        public static async Task<Result> DumbBadging(string pathApk)
        {
            var output = string.Empty;

            try
            {
                output = await RunAsync("dump badging \"{0}\"", pathApk);
            }
            catch (Exception e)
            {
                Debug.Print("Aapt.DumbBadging: {0}", e.Message);
                return new Result()
                {
                    Success = false,
                    Message = e.Message
                };
            }

            var apk = new Apk() { FilePath = pathApk };

            //package
            var match = Regex.Match(output, "package: name='(.+?)'");
            if (match.Success) apk.PackageName = match.Groups[1].Value;

            //versionCode
            match = Regex.Match(output, "package.+versionCode='(.+?)'");
            if (match.Success)
            {
                double.TryParse(match.Groups[1].Value, out double versionCode);
                apk.VersionCode = versionCode;
            }

            //versionName
            match = Regex.Match(output, "package.+versionName='(.+?)'");
            if (match.Success) apk.VersionName = match.Groups[1].Value;

            //label
            match = Regex.Match(output, "application.+label='(.+?)'");
            if (match.Success) apk.Label = match.Groups[1].Value;

            //icon
            match = Regex.Match(output, "application.+icon='(.+?)'");
            if (match.Success) apk.Icon = GetIconFrom(pathApk, match.Groups[1].Value);

            //sdkVersion
            match = Regex.Match(output, @"sdkVersion:'(\d+?)'");
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out int sdkVersion);
                apk.SdkVersion = sdkVersion;
            }

            //targetSdkVersion
            match = Regex.Match(output, @"targetSdkVersion:'(\d+?)'");
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out int targetSdkVersion);
                apk.TargetSdkVersion = targetSdkVersion;
            }

            //permission
            var matches = Regex.Matches(output, "uses-permission: name='(.+?)'");
            foreach (Match m in matches) apk.Permissions.Add(m.Groups[1].Value);

            //native-code
            match = Regex.Match(output, "native-code: '(.+)'");
            if (match.Success)
            {
                apk.AbiList = match.Groups[1].Value.Replace("' '",", ");
                apk.Platforms = match.Groups[1].Value.Replace("' '", " ").Split(' ').ToList();
            }
            else
            {
                apk.AbiList = "any";
                apk.Platforms.Add("any");
            }

            //launchable-activity
            match = Regex.Match(output, "launchable-activity: name='(.+?)'");
            if (match.Success)
            {
                apk.LaunchableActivity = match.Groups[1].Value;
            }

            return new Result()
            {
                Apk = apk,
                Success = true
            };
        }
    }
}

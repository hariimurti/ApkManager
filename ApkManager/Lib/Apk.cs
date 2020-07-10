using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace ApkManager.Lib
{
    public class Apk
    {
        public string PackageName { get; set; }
        public string Label { get; set; }
        public string VersionName { get; set; }
        public double VersionCode { get; set; }
        public int SdkVersion { get; set; }
        public int TargetSdkVersion { get; set; }
        public BitmapImage Icon { get; set; }
        public IList<string> Permissions { get; set; }
        public IList<string> Platforms { get; set; }
        public string AbiList { get; set; }
        public string FilePath { get; set; }
        public string LaunchableActivity { get; set; }

        public Apk()
        {
            Permissions = new List<string>();
            Platforms = new List<string>();
        }

        public bool isAbiCompatible(Adb.Device device)
        {
            if (AbiList == "any-cpu") return true;

            if (device.Arch == "armeabi-v7a")
            {
                return AbiList.Contains("armeabi");
            }

            if (device.Arch == "arm64-v8a")
            {
                return AbiList.Contains("arm");
            }

            if (device.Arch == "x86_64")
            {
                return AbiList.Contains("x86");
            }

            return AbiList.Contains(device.Arch);
        }

        public bool isSdkCompatible(Adb.Device device)
        {
            return SdkVersion <= device.Sdk;
        }

        public bool canInstallTo(Adb.Device device)
        {
            return isAbiCompatible(device) && isSdkCompatible(device);
        }
    }
}

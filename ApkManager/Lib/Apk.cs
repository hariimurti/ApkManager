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
    }
}

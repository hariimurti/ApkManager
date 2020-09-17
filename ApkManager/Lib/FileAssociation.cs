using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace ApkManager
{
    class FileAssociation
    {
        private static readonly string APK_EXTENSION = ".apk";
        private static readonly string APK_FILE = "apkfile";
        private static readonly string APK_DESCRIPTION = "Android Application";
        private static readonly RegistryKey Root = Registry.ClassesRoot;
        private static readonly string ExePath = Path.GetFullPath(System.Reflection.Assembly.GetEntryAssembly().Location);

        public static bool SetAsDefault(bool value)
        {
            try
            {
                if (value)
                {
                    Root.CreateSubKey(APK_EXTENSION).SetValue("", APK_FILE);
                    using (var key = Root.CreateSubKey(APK_FILE))
                    {
                        key.SetValue("", APK_DESCRIPTION);
                        key.CreateSubKey("DefaultIcon").SetValue("", $"\"{ExePath}\"");
                        key.CreateSubKey("Shell\\Open\\Command").SetValue("", $"\"{ExePath}\" \"%1\"");
                    }
                }
                else
                {
                    Root.DeleteSubKeyTree(APK_EXTENSION);
                    Root.DeleteSubKeyTree(APK_FILE);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Registry: {0}", e.Message);
                return false;
            }
        }

        public static bool IsDefault()
        {
            try
            {
                using (var key = Root.OpenSubKey(APK_EXTENSION, false))
                {
                    if (key == null)
                        return false;

                    if (key.GetValue("").ToString() != APK_FILE)
                        return false;
                }

                using (var key = Root.OpenSubKey(APK_FILE, false))
                {
                    if (key == null)
                        return false;

                    var regValue = key.OpenSubKey("Shell\\Open\\Command")?.GetValue("");

                    if (regValue == null)
                        return false;

                    if (regValue.ToString() != $"\"{ExePath}\" \"%1\"")
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Registry: {0}", e.Message);
                return false;
            }
        }
    }
}

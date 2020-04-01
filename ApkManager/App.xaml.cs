using ApkManager.Lib;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ApkManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var cfg = new Config();
            using (var mutex = new Mutex(true, "{BB8C82B6-7851-46A0-A902-48446B59CAD6}", out bool isFirstInstance))
            {
                if (!isFirstInstance && cfg.SingleInstance()) return;

                var apks = e.Args.Where(s => s.ToLower().EndsWith(".apk")).ToArray();

                if (apks.Count() <= 1)
                    new MainWindow(apks.FirstOrDefault()).ShowDialog();
            }
        }

        public static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static BitmapImage GetImageFromResources(string pathResource)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly().GetName().Name;
                var packUri = string.Format("pack://application:,,,/{0};component/{1}", assembly, pathResource);
                return new BitmapImage(new Uri(packUri));
            }
            catch (Exception e)
            {
                Debug.Print("App.GetImageResource: {0}", e.Message);
                return null;
            }
        }

        public static BitmapImage GetPlaystoreImageFromResources()
        {
            return GetImageFromResources("Resources/Playstore.png");
        }
    }
}

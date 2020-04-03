using ApkManager.Lib;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
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
        public static readonly string PIPENAMESERVER = "{BB8C82B6-7851-46A0-A902-48446B59CAD6}";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var cfg = new Config();
            using (var mutex = new Mutex(true, PIPENAMESERVER, out bool isFirstInstance))
            {
                // collect all args where apk & exist
                var apks = e.Args.Where(s => s.ToLower().EndsWith(".apk") && File.Exists(s)).ToList();
                if (apks.Count == 0) apks.Add("");

                if (!isFirstInstance && cfg.SingleInstance())
                {
                    // send message to existed app
                    new PipeClient(PIPENAMESERVER).SendMessage(apks.FirstOrDefault()).GetAwaiter();
                    Current.Shutdown();
                }
                else
                {
                    // open main window with null or single apk
                    new MainWindow(apks.FirstOrDefault()).ShowDialog();
                }
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

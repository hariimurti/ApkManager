using ApkManager.Lib;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApkManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Config cfg;
        private Apk loadedApk;
        private string pathApk;
        public bool donotInterupt;

        #region WINDOW
        public MainWindow(string pathApk = null)
        {
            InitializeComponent();

            // make all flyout width same as root
            foreach (Flyout flayout in Flyouts.Items)
                flayout.Width = this.Width;
            
            cfg = new Config();
            this.pathApk = pathApk;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // loading position
            if (cfg.GetWindowPostition())
            {
                var position = cfg.WindowPostition();
                if (position.Top!= 0 && position.Left != 0)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Top = position.Top;
                    this.Left = position.Left;
                }
            }

            // loading switch settings
            swHander.IsEnabled = App.IsAdministrator();
            swHander.IsChecked = FileAssociation.IsDefault();
            swInstance.IsChecked = cfg.SingleInstance();
            swWindow.IsChecked = cfg.GetWindowPostition();

            // open apk passes arg not null
            if (!string.IsNullOrWhiteSpace(pathApk))
                txtPath.Text = pathApk;

            // start pip server if in one single instance
            if (cfg.SingleInstance())
            {
                var server = new PipeServer(App.PIPENAMESERVER);
                server.OnMessageReceived += OnMessageReceived;
                server.StartListening();
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cfg.GetWindowPostition())
                cfg.WindowPostition(new WindowPosition() { Top = this.Top, Left = this.Left });
        }

        private void OnMessageReceived(string message)
        {
            if (donotInterupt) return;

            txtPath.Text = message;

            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            this.Activate();
        }

        private void OnFileDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            var apk = files.Where(f => f.ToLower().EndsWith(".apk")).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(apk))
                txtPath.Text = apk;
        }
        #endregion

        #region METHOD
        private void ShowLoading(bool state = true)
        {
            Dispatcher.Invoke(() =>
                PanelLoading.Visibility = state ? Visibility.Visible : Visibility.Collapsed
            );
        }
        #endregion

        private async void TxtPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!(sender is TextBox tb)) return;

            txtLabel.Text = ". . . .";
            txtLabel.ToolTip = null;
            txtPackage.Text = ". . . .";
            txtPackage.ToolTip = null;
            txtVersion.Text = ". . . .";
            txtVersion.ToolTip = null;
            txtAbi.Text = ". . . .";
            txtAbi.ToolTip = null;
            txtSdk.Text = ". . . .";
            imgIcon.Source = App.GetPlaystoreImageFromResources();
            gbAction.IsEnabled = false;

            if (string.IsNullOrWhiteSpace(tb.Text)) return;
            if (loadedApk == null || loadedApk.FilePath != tb.Text)
            {
                ShowLoading();

                var aapt = await Aapt.DumbBadging(tb.Text);
                if (aapt.Success)
                {
                    loadedApk = aapt.Apk;

                    txtLabel.Text = loadedApk.Label;
                    txtLabel.ToolTip = txtLabel.Text;

                    txtPackage.Text = loadedApk.PackageName;
                    txtPackage.ToolTip = txtPackage.Text;

                    txtVersion.Text = string.Format("{0} ( {1} )", loadedApk.VersionName, loadedApk.VersionCode);
                    txtVersion.ToolTip = txtVersion.Text;

                    txtAbi.Text = loadedApk.AbiList;
                    txtAbi.ToolTip = txtAbi.Text;

                    txtSdk.Text = loadedApk.SdkVersion.ToString();

                    imgIcon.Source = loadedApk.Icon;
                    gbAction.IsEnabled = true;
                }
                // file corrupt or error
                else
                {
                    txtLabel.Text = "file corrupt?";
                    txtPackage.Text = "not an apk file? unusual file path?";
                }

                ShowLoading(false);
            }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var open = new OpenFileDialog()
            {
                Title = "Select APK",
                Filter = "Android Package|*.apk",
                DefaultExt = "apk",
                RestoreDirectory = true,
                Multiselect = false
            };

            if (open.ShowDialog() == true)
                txtPath.Text = open.FileName;
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            menuSettings.IsOpen = !menuSettings.IsOpen;
        }

        private void SwitchSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleSwitch ts)) return;
            var tag = ts.Tag as string;
            var isChecked = ts.IsChecked == true;
            
            if (tag == "Handler")
                ts.IsChecked = FileAssociation.SetAsDefault(isChecked);
            if (tag == "Instance")
                cfg.SingleInstance(isChecked);
            if (tag == "Window")
                cfg.SetWindowPostition(isChecked);
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            txtPath.Text = string.Empty;
        }

        private async void ButtonRenamer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                donotInterupt = true;

                var window = new RenamerWindow(loadedApk);
                if (window.ShowDialog().Value == false) return;

                var newdest = window.GetFileDestination();
                File.Move(loadedApk.FilePath, newdest);
                txtPath.Text = newdest;
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Renamer", ex.Message);
            }
            finally
            {
                donotInterupt = false;
            }
        }

        private void ButtonInstaller_Click(object sender, RoutedEventArgs e)
        {
            donotInterupt = true;

            new AdbWindow(loadedApk).ShowDialog();

            donotInterupt = false;
        }
    }
}
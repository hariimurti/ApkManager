using ApkManager.Lib;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kind = MahApps.Metro.IconPacks.PackIconMaterialKind;

namespace ApkManager
{
    /// <summary>
    /// Interaction logic for AdbWindow.xaml
    /// </summary>
    public partial class AdbWindow : MetroWindow
    {
        private Config cfg;
        private Adb adb;
        private Apk apk;
        private bool isLoading = false;
        
        public enum ShowMenu { Main, Install, Uninstall }

        #region WINDOW
        public AdbWindow(Apk apk)
        {
            InitializeComponent();

            // make all flyout width same as root
            foreach (Flyout flayout in Flyouts.Items)
                flayout.Width = this.Width;

            // set switch in settings
            cfg = new Config();
            swClose.IsChecked = cfg.AutoClose();

            // define adb
            adb = new Adb();
            adb.OnProcess += (value) => ShowLoading(value);
            adb.OutputDataReceived += (msg) => CommandOutput_Insert(msg, false);
            adb.ErrorDataReceived += (msg) => CommandOutput_Insert(msg, true);

            this.apk = apk;
        }

        private void ShowLoading(bool state = true)
        {
            Dispatcher.Invoke(() => {
                isLoading = state;
                PanelLoading.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // refresh list
            ButtonRefresh_Click(sender, e);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = isLoading;
        }
        #endregion

        #region TOPBAR
        private async void WindowCommand_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            switch ((string)btn.Tag)
            {
                case "reconnect":
                    await adb.Reconnect();
                    ButtonRefresh_Click(null, null);
                    break;
                case "settings":
                    menuSettings.IsOpen = !menuSettings.IsOpen;
                    break;
            }
        }

        private void SwitchSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleSwitch ts)) return;

            var tag = ts.Tag as string;
            if (tag == "Close")
                cfg.AutoClose(ts.IsChecked == true);
        }
        #endregion

        #region DEVICES
        private async void ComboDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            var address = combo.SelectedItem as string;

            txtDevice.Text = ". . . .";
            txtAndroid.Text = ". . . .";
            txtArch.Text = ". . . .";
            txtSdk.Text = ". . . .";
            btnMenuInstall.IsEnabled = false;

            if (!string.IsNullOrWhiteSpace(address))
            {
                var device = await adb.GetDevice(address);
                var supported = apk.SdkVersion <= device.Sdk;

                txtDevice.Text = device.Name;
                txtAndroid.Text = device.Android;
                txtArch.Text = device.Arch;
                txtSdk.Text = device.Sdk.ToString();
                txtSdk.Foreground = supported ? txtArch.Foreground : Brushes.Red;
                btnMenuInstall.IsEnabled = supported;
            }
        }

        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            cbDevices.Items.Clear();

            var devices = await adb.GetDevices();
            foreach(var device in devices)
            {
                cbDevices.Items.Add(device);
            }

            cbDevices.SelectedIndex = 0;
        }

        private void ButtonWireless_Click(object sender, RoutedEventArgs e)
        {
            var selected = cbDevices.Text;
            var last = cfg.LastAddress();
            if (selected.IsValidIPAddress())
                txtAddress.Text = selected;
            else if (!string.IsNullOrWhiteSpace(last))
                txtAddress.Text = last;

            menuWifi.IsOpen = true;
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text)) return;

            menuWifi.IsOpen = false;

            var result = await adb.Connect(txtAddress.Text);
            if (result)
            {
                if (cfg.LastAddress() != txtAddress.Text)
                    cfg.LastAddress(txtAddress.Text);
                
                ButtonRefresh_Click(null, null);
            }
            else menuWifi.IsOpen = true;
        }

        private async void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text)) return;

            menuWifi.IsOpen = false;

            var result = await adb.Disconnect(txtAddress.Text);
            if (result) ButtonRefresh_Click(null, null);
            else menuWifi.IsOpen = true;
        }

        private void TextAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                var isValid = tb.Text.IsValidIPAddress();
                IconAddress.Kind = isValid ? Kind.Wifi : Kind.WifiOff;
                ButtonConnect.IsEnabled = isValid;
                ButtonDisconnect.IsEnabled = isValid;
            }
            else
            {
                IconAddress.Kind = Kind.WifiOff;
                ButtonConnect.IsEnabled = false;
                ButtonDisconnect.IsEnabled = false;
            }
        }
        #endregion

        #region OUTPUT
        private void CommandOutput_Reset()
        {
            Dispatcher.Invoke(() => lbOutput.Items.Clear());
        }

        private void CommandOutput_Insert(string message, bool error = false)
        {
            Dispatcher.Invoke(() =>
            {
                var color = Brushes.WhiteSmoke;
                if (error || message.ToLower().Contains("failed"))
                    color = Brushes.IndianRed;
                else if (message.ToLower().Contains("success"))
                    color = Brushes.LightBlue;

                var item = new ListBoxItem()
                {
                    Content = message.Trim(),
                    Background = color
                };
                lbOutput.Items.Add(item);
                lbOutput.ScrollIntoView(item);
            });
        }
        #endregion

        #region ACTION
        private void ShowActionMenu(ShowMenu menu)
        {
            if (menu == ShowMenu.Install)
            {
                wcReconnect.Visibility = Visibility.Collapsed;

                actionMain.Visibility = Visibility.Collapsed;
                actionInstall.Visibility = Visibility.Visible;
                actionUninstall.Visibility = Visibility.Collapsed;

                CommandOutput_Reset();
                CommandOutput_Insert($"File APK : {Path.GetFileName(apk.FilePath)}");

                gbTarget.IsEnabled = false;
                gbAction.Header = "Action : Install";
                gbCommand.Visibility = Visibility.Visible;
            }
            else if (menu == ShowMenu.Uninstall)
            {
                wcReconnect.Visibility = Visibility.Collapsed;

                actionMain.Visibility = Visibility.Collapsed;
                actionInstall.Visibility = Visibility.Collapsed;
                actionUninstall.Visibility = Visibility.Visible;

                CommandOutput_Reset();
                CommandOutput_Insert($"Package : {apk.PackageName}");

                gbTarget.IsEnabled = false;
                gbAction.Header = "Action : Uninstall";
                gbCommand.Visibility = Visibility.Visible;
            }
            else
            {
                wcReconnect.Visibility = Visibility.Visible;

                actionMain.Visibility = Visibility.Visible;
                actionInstall.Visibility = Visibility.Collapsed;
                actionUninstall.Visibility = Visibility.Collapsed;

                gbTarget.IsEnabled = true;
                gbAction.Header = "Select Action";
                gbCommand.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonMenuInstall_Click(object sender, RoutedEventArgs e)
        {
            ShowActionMenu(ShowMenu.Install);
        }

        private void ButtonMenuUninstall_Click(object sender, RoutedEventArgs e)
        {
            ShowActionMenu(ShowMenu.Uninstall);
        }

        private void ButtonMenuBack_Click(object sender, RoutedEventArgs e)
        {
            ShowActionMenu(ShowMenu.Main);
        }

        private async void ButtonActionInstall_Click(object sender, RoutedEventArgs e)
        {
            var result = await adb.Install(cbDevices.Text, apk.FilePath);
            if (result && cfg.AutoClose())
                this.Close();
        }

        private async void ButtonActionUninstall_Click(object sender, RoutedEventArgs e)
        {
            CommandOutput_Insert("Force remove app and data....");
            await adb.Uninstall(cbDevices.Text, apk.PackageName);
        }

        private async void ButtonActionUninstallKeep_Click(object sender, RoutedEventArgs e)
        {
            CommandOutput_Insert("Remove app but keep the data....");
            await adb.Uninstall(cbDevices.Text, apk.PackageName, true);
        }
        #endregion
    }
}

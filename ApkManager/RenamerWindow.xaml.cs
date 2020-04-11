using ApkManager.Lib;
using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Separator = ApkManager.Lib.Separator;

namespace ApkManager
{
    /// <summary>
    /// Interaction logic for RenamerWindow.xaml
    /// </summary>
    public partial class RenamerWindow : MetroWindow
    {
        private Apk apk;
        private Config config;
        private string destination;

        public RenamerWindow(Apk apk)
        {
            InitializeComponent();

            this.apk = apk;
            this.config = new Config();
        }

        public string GetFileDestination()
        {
            return destination;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var name = Path.GetFileName(apk.FilePath);
            tbSource.Text = name;
            tbDestination.Text = name;

            // get config
            var nf = config.GetNameFormat();

            // set pattern
            tbPattern.Text = nf.UsePattern;

            // set base
            cbBaseLabel.IsChecked = nf.UseLabel;
            cbBasePackage.IsChecked = nf.UsePackage;
            cbBaseVersion.IsChecked = nf.UseVersion;
            cbBaseBuild.IsChecked = nf.UseBuild;
            // make sure name base not empty
            if (!nf.UseLabel && !nf.UsePackage)
                cbBaseLabel.IsChecked = true;

            // set suffix
            var tuple = new Tuple<string, string>(name, apk.Label);
            cbSuffixPro.IsChecked = tuple.IsMatchInTextAndNotInLabel("pro");
            cbSuffixPremium.IsChecked = tuple.IsMatchInTextAndNotInLabel("premium");
            cbSuffixPaid.IsChecked = tuple.IsMatchInTextAndNotInLabel("paid");
            cbSuffixDonate.IsChecked = tuple.IsMatchInTextAndNotInLabel("donate");
            cbSuffixVip.IsChecked = tuple.IsMatchInTextAndNotInLabel("vip");
            cbSuffixFull.IsChecked = tuple.IsMatchInTextAndNotInLabel("full");
            cbSuffixPatched.IsChecked = tuple.IsMatchInTextAndNotInLabel("patched");
            cbSuffixUnlocked.IsChecked = tuple.IsMatchInTextAndNotInLabel("unlock(ed)?");
            cbSuffixMod.IsChecked = tuple.IsMatchInTextAndNotInLabel("mod(ded)?");
            cbSuffixAdFree.IsChecked = tuple.IsMatchInTextAndNotInLabel("ad[-| ]?free");
            cbSuffixLite.IsChecked = tuple.IsMatchInTextAndNotInLabel("lite");
            cbSuffixFinal.IsChecked = tuple.IsMatchInTextAndNotInLabel("final");
            cbSuffixBeta.IsChecked = tuple.IsMatchInTextAndNotInLabel("beta");

            // set separator
            cbSuffixEnclosure.IsChecked = nf.UseSuffixEnclosure;
            switch (nf.Separator)
            {
                case Separator.Strip:
                    rbSeparatorStrip.IsChecked = true;
                    break;
                case Separator.Underscore:
                    rbSeparatorUnderscore.IsChecked = true;
                    break;
                default:
                    rbSeparatorSpace.IsChecked = true;
                    break;
            }

            // preview changed
            CheckChanged_Click(sender, e);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;

            // check if label & package is not used
            if (cb.Content == cbBaseLabel.Content && cb.IsChecked == false && cbBasePackage.IsChecked == false)
                cbBasePackage.IsChecked = true;
            if (cb.Content == cbBasePackage.Content && cb.IsChecked == false && cbBaseLabel.IsChecked == false)
                cbBaseLabel.IsChecked = true;

            CheckChanged_Click(sender, e);
        }
        
        private void CheckChanged_Click(object sender, RoutedEventArgs e)
        {
            // base
            var namebase = string.Empty;
            if (cbBaseLabel.IsChecked == true)
                namebase = namebase.Append(apk.Label);
            if (cbBasePackage.IsChecked == true)
                namebase = namebase.Append(apk.PackageName);
            if (cbBaseVersion.IsChecked == true)
                namebase = namebase.Append((apk.VersionName.ToLower().StartsWith("v") ? "" : "v") + apk.VersionName);
            if (cbBaseBuild.IsChecked == true)
                namebase = namebase.Append("b" + apk.VersionCode);

            // base final
            if (rbSeparatorStrip.IsChecked == true)
                namebase = namebase.Replace(" ", "-");
            if (rbSeparatorUnderscore.IsChecked == true)
                namebase = namebase.Replace(" ", "_");
            
            // suffix
            var namesuffix = string.Empty;
            if (cbSuffixPro.IsChecked == true)
                namesuffix = namesuffix.Append("Pro");
            if (cbSuffixPremium.IsChecked == true)
                namesuffix = namesuffix.Append("Premium");
            if (cbSuffixPaid.IsChecked == true)
                namesuffix = namesuffix.Append("Paid");
            if (cbSuffixDonate.IsChecked == true)
                namesuffix = namesuffix.Append("Donate");
            if (cbSuffixVip.IsChecked == true)
                namesuffix = namesuffix.Append("VIP");
            if (cbSuffixFull.IsChecked == true)
                namesuffix = namesuffix.Append("Full");
            if (cbSuffixPatched.IsChecked == true)
                namesuffix = namesuffix.Append("Patched");
            if (cbSuffixUnlocked.IsChecked == true)
                namesuffix = namesuffix.Append("Unlocked");
            if (cbSuffixMod.IsChecked == true)
                namesuffix = namesuffix.Append("Mod");
            if (cbSuffixAdFree.IsChecked == true)
                namesuffix = namesuffix.Append("AdFree");
            if (cbSuffixLite.IsChecked == true)
                namesuffix = namesuffix.Append("Lite");
            if (cbSuffixFinal.IsChecked == true)
                namesuffix = namesuffix.Append("Final");
            if (cbSuffixBeta.IsChecked == true)
                namesuffix = namesuffix.Append("Beta");

            // suffix enclosure
            var useEnclosure = cbSuffixEnclosure.IsChecked == true;
            if (useEnclosure)
            {
                namesuffix = Regex.Replace(namesuffix, "(\\w+)", delegate (Match match) {
                    return string.Format("[{0}]", match.Groups[1].Value);
                }).Replace(" ","");
            }

            // suffix abi
            var filename = Path.GetFileName(apk.FilePath);
            if (filename.IsMatch("armeabi-v7a"))
                namesuffix = namesuffix.Append(useEnclosure ? "[armeabi-v7a]" : "armeabi-v7a", !useEnclosure);
            if (filename.IsMatch("arm64-v8a"))
                namesuffix = namesuffix.Append(useEnclosure ? "[arm64-v8a]" : "arm64-v8a", !useEnclosure);
            if (filename.IsMatch("x86_x64"))
                namesuffix = namesuffix.Append(useEnclosure ? "[x86_x64]" : "x86_x64", !useEnclosure);
            else if (filename.IsMatch("x86"))
                namesuffix = namesuffix.Append(useEnclosure ? "[x86]" : "x86", !useEnclosure);

            // suffix final
            if (!useEnclosure)
            {
                if (rbSeparatorStrip.IsChecked == true)
                    namesuffix = namesuffix.Replace(" ", "-");
                if (rbSeparatorUnderscore.IsChecked == true)
                    namesuffix = namesuffix.Replace(" ", "_");
            }

            // define final name
            var name = string.Empty;

            // pattern override
            if (!string.IsNullOrWhiteSpace(tbPattern.Text))
            {
                name = tbPattern.Text
                    .Replace("%label%", apk.Label)
                    .Replace("%package%", apk.PackageName)
                    .Replace("%version%", apk.VersionName)
                    .Replace("%build%", apk.VersionCode.ToString())
                    .Replace("%base%", namebase)
                    .Replace("%suffix%", namesuffix);
            }
            else
            {
                var separator = "";
                if (rbSeparatorSpace.IsChecked == true)
                    separator = " ";
                if (rbSeparatorStrip.IsChecked == true && cbSuffixEnclosure.IsChecked == false)
                    separator = "-";
                if (rbSeparatorUnderscore.IsChecked == true && cbSuffixEnclosure.IsChecked == false)
                    separator = "_";

                name = string.Format("{0}{1}{2}", namebase, separator, namesuffix).Trim();
            }
            
            // set new name
            tbDestination.Text = name.AppendExtApk().TrimInvalidFileNameChars();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var invalidchars = Path.GetInvalidFileNameChars().Where(c => e.Text.Where(t => t == c).Count() > 0);
            e.Handled = invalidchars.Count() > 0;
            base.OnPreviewTextInput(e);
        }

        private void TbPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckChanged_Click(sender, e);
        }

        private void TbDestination_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            btnRenamer.IsEnabled = false;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                try
                {
                    var filename = tb.Text.AppendExtApk();
                    var folder = Path.GetDirectoryName(apk.FilePath);
                    destination = Path.Combine(folder, filename);

                    if (!File.Exists(destination))
                        btnRenamer.IsEnabled = true;
                }
                catch (Exception) { }
            }
        }

        private void ButtonRename_Click(object sender, RoutedEventArgs e)
        {
            var nf = new NameFormat()
            {
                UsePattern = tbPattern.Text,
                UseLabel = cbBaseLabel.IsChecked == true,
                UsePackage = cbBasePackage.IsChecked == true,
                UseVersion = cbBaseVersion.IsChecked == true,
                UseBuild = cbBaseBuild.IsChecked == true,
                UseSuffixEnclosure = cbSuffixEnclosure.IsChecked == true
            };
            if (rbSeparatorSpace.IsChecked == true)
                nf.Separator = Separator.Space;
            if (rbSeparatorStrip.IsChecked == true)
                nf.Separator = Separator.Strip;
            if (rbSeparatorUnderscore.IsChecked == true)
                nf.Separator = Separator.Underscore;
            config.SetNameFormat(nf);

            this.DialogResult = true;
        }
    }
}

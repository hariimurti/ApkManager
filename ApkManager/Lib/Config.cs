using System;
using System.Configuration;
using System.Diagnostics;

namespace ApkManager.Lib
{
    class Config
    {
        private Configuration config;

        #region KEY STRING
        private static readonly string KEY_APPSETTINGS = "appSettings";
        // Window
        private static readonly string KEY_SINGLEINSTANCE = "SingleInstance";
        private static readonly string KEY_SAVEWINDOWPOSITION = "SaveWindowPosition";
        private static readonly string KEY_WINDOWMAIN = "WindowMain";
        // Last used
        private static readonly string KEY_LASTADDRESS = "LastAddress";
        private static readonly string KEY_AUTOCLOSE = "AutoClose";
        // Usage
        private static readonly string KEY_PATTERN = "UsePattern";
        private static readonly string KEY_LABEL = "UseLabel";
        private static readonly string KEY_PACKAGE = "UsePackage";
        private static readonly string KEY_VERSION = "UseVersion";
        private static readonly string KEY_BUILD = "UseBuild";
        private static readonly string KEY_SUFFIXENCLOSURE = "UseSuffixEnclosure";
        private static readonly string KEY_SEPARATOR = "Separator";
        #endregion

        #region SET VALUE
        private bool SetValue(string key, string value)
        {
            try
            {
                config.AppSettings.Settings[key].Value = value;
                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection(KEY_APPSETTINGS);
                return true;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                return false;
            }
        }

        private bool SetValue(string key, bool value)
        {
            return SetValue(key, value.ToString());
        }

        private bool SetValue(string key, int value)
        {
            return SetValue(key, value.ToString());
        }

        private bool SetValue(string key, double value)
        {
            return SetValue(key, value.ToString());
        }
        #endregion

        #region GET VALUE
        private string GetString(string key)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception($"Config: key \"{key}\" is not found!");
                else
                    return value;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                return string.Empty;
            }
        }

        private bool GetBoolean(string key)
        {
            return GetString(key).ToLower() == "true";
        }

        private int GetInteger(string key)
        {
            var value = GetString(key);
            int.TryParse(value, out int num);
            return num;
        }

        private double GetDouble(string key)
        {
            var value = GetString(key);
            double.TryParse(value, out double num);
            return num;
        }
        #endregion

        #region GET-SET CONFIG
        public Config()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public bool SingleInstance()
        {
            return GetBoolean(KEY_SINGLEINSTANCE);
        }

        public bool SingleInstance(bool value)
        {
            return SetValue(KEY_SINGLEINSTANCE, value);
        }

        public WindowPosition WindowPostition()
        {
            try
            {
                var value = GetString(KEY_WINDOWMAIN).Split(',');
                double.TryParse(value[0], out double top);
                double.TryParse(value[1], out double left);
                return new WindowPosition() { Top = top, Left = left };
            }
            catch (Exception e)
            {
                Debug.Print("Config.WindowPostition: {0}", e.Message);
                return new WindowPosition() { Top = 0, Left = 0 };
            }
        }

        public bool WindowPostition(WindowPosition position)
        {
            var window = string.Format("{0},{1}", position.Top, position.Left);
            return SetValue(KEY_WINDOWMAIN, window);
        }

        public bool GetWindowPostition()
        {
            return GetBoolean(KEY_SAVEWINDOWPOSITION);
        }

        public bool SetWindowPostition(bool value)
        {
            return SetValue(KEY_SAVEWINDOWPOSITION, value);
        }

        public string LastAddress()
        {
            return GetString(KEY_LASTADDRESS);
        }

        public bool LastAddress(string address)
        {
            return SetValue(KEY_LASTADDRESS, address);
        }

        public bool AutoClose()
        {
            return GetBoolean(KEY_AUTOCLOSE);
        }

        public bool AutoClose(bool value)
        {
            return SetValue(KEY_AUTOCLOSE, value);
        }

        public NameFormat GetNameFormat()
        {
            return new NameFormat
            {
                UsePattern = GetString(KEY_PATTERN),
                UseLabel = GetBoolean(KEY_LABEL),
                UsePackage = GetBoolean(KEY_PACKAGE),
                UseVersion = GetBoolean(KEY_VERSION),
                UseBuild = GetBoolean(KEY_BUILD),
                UseSuffixEnclosure = GetBoolean(KEY_SUFFIXENCLOSURE),
                Separator = (Separator)GetInteger(KEY_SEPARATOR)
            };
        }

        public bool SetNameFormat(NameFormat nf)
        {
            var pattern = SetValue(KEY_PATTERN, nf.UsePattern);
            var label = SetValue(KEY_LABEL, nf.UseLabel);
            var package = SetValue(KEY_PACKAGE, nf.UsePackage);
            var version = SetValue(KEY_VERSION, nf.UseVersion);
            var build = SetValue(KEY_BUILD, nf.UseBuild);
            var enclosure = SetValue(KEY_SUFFIXENCLOSURE, nf.UseSuffixEnclosure);
            var separator = SetValue(KEY_SEPARATOR, (int)nf.Separator);
            return pattern && label && package && version && build && enclosure && separator;
        }
        #endregion
    }
}

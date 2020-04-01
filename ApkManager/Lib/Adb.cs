using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApkManager
{
    internal class Adb
    {
        public class Device
        {
            public string Name { get; set; }
            public string Android { get; set; }
            public string Arch { get; set; }
            public int Sdk { get; set; }

            public override string ToString()
            {
                return Name.ToString();
            }
        }

        private static bool OVERRIDE_ONPROCESSEVENT = false;

        public delegate void ProcessEventHandler(bool value);

        public event ProcessEventHandler OnProcess;

        public delegate void OutputEventHander(string message);

        public event OutputEventHander OutputDataReceived;

        public delegate void ErrorEventHandler(string message);

        public event ErrorEventHandler ErrorDataReceived;

        public Adb()
        {
        }

        private async Task<string> RunAsync(string command, params object[] args)
        {
            return await Task.Run(() =>
            {
                using (var p = new Process())
                {
                    var OutputMessage = string.Empty;
                    p.OutputDataReceived += (s, e) => {
                        if (string.IsNullOrWhiteSpace(e.Data)) return;
                        OutputMessage += e.Data + Environment.NewLine;
                        OutputDataReceived?.Invoke(e.Data);
                    };

                    var ErrorMessage = string.Empty;
                    p.ErrorDataReceived += (s, e) => {
                        if (string.IsNullOrWhiteSpace(e.Data)) return;
                        ErrorMessage += e.Data + Environment.NewLine;
                        ErrorDataReceived?.Invoke(e.Data);
                    };

                    p.StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = Path.Combine("Lib", "adb.exe"),
                        Arguments = string.Format(command, args)
                    };

                    p.Start();

                    Debug.Print("Adb.Command: adb {0}", string.Format(command, args));

                    if (!OVERRIDE_ONPROCESSEVENT)
                        OnProcess?.Invoke(true);

                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    
                    p.WaitForExit();

                    if (!OVERRIDE_ONPROCESSEVENT)
                        OnProcess?.Invoke(false);

                    if (p.ExitCode != 0) throw new Exception(ErrorMessage);

                    Debug.Print("Adb.Output: {0}", OutputMessage);
                    return OutputMessage;
                };
            });
        }

        public async Task<bool> StartServer()
        {
            try
            {
                await RunAsync("start-server");
                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Adb.StartServer: {0}", e.Message);
                return false;
            }
        }

        public async Task<bool> Reconnect()
        {
            try
            {
                await RunAsync("reconnect");
                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Adb.Reconnect: {0}", e.Message);
                return false;
            }
        }

        public async Task<bool> Connect(string address)
        {
            try
            {
                if (!address.Contains(":"))
                    address += ":5555";

                var result = await RunAsync("connect {0}", address);
                return result.Contains("connected");
            }
            catch (Exception e)
            {
                Debug.Print("Adb.Connect: {0}", e.Message);
                return false;
            }
        }

        public async Task<bool> Disconnect(string address)
        {
            try
            {
                if (!address.Contains(":"))
                    address += ":5555";

                var result = await RunAsync("disconnect {0}", address);
                return result.Contains("disconnected") || result.Contains("no such device");
            }
            catch (Exception e)
            {
                Debug.Print("Adb.Disconnect: {0}", e.Message);
                return false;
            }
        }

        public async Task<List<string>> GetDevices()
        {
            var devices = new List<string>();

            try
            {
                var result = await RunAsync("devices");
                var matches = Regex.Matches(result, @"(.+?)\tdevice");
                foreach (Match match in matches)
                {
                    devices.Add(match.Groups[1].Value);
                }
            }
            catch (Exception e)
            {
                Debug.Print("Adb.GetDevices: {0}", e.Message);
            }

            return devices;
        }

        public async Task<bool> Install(string device, string pathApk)
        {
            try
            {
                var result = await RunAsync("-s {0} install -r \"{1}\"", device, pathApk);
                return result.Contains("Success");
            }
            catch (Exception e)
            {
                Debug.Print("Adb.Install: {0}", e.Message);
                return false;
            }
        }

        public async Task<bool> Uninstall(string device, string package, bool keepData = false)
        {
            try
            {
                var command = keepData ? $"-s {device} shell pm uninstall -k {package}" : $"-s {device} uninstall {package}";
                var result = await RunAsync(command);
                return result.Contains("Success");
            }
            catch (Exception e)
            {
                Debug.Print("Adb.Uninstall: {0}", e.Message);
                return false;
            }
        }

        public async Task<string> GetProp(string device, string prop)
        {
            var result = await RunAsync("-s {0} shell getprop {1}", device, prop);
            return string.IsNullOrWhiteSpace(result) ? "Unknown" : result.Trim();
        }

        public async Task<Device> GetDevice(string address)
        {
            OVERRIDE_ONPROCESSEVENT = true;
            OnProcess?.Invoke(true);

            var name = await GetProp(address, "ro.product.model");
            if (string.IsNullOrWhiteSpace(name))
                name = await GetProp(address, "ro.product.brand");
            if (string.IsNullOrWhiteSpace(name))
                name = await GetProp(address, "ro.product.device");
            if (string.IsNullOrWhiteSpace(name))
                name = "Unknown";

            var android = await GetProp(address, "ro.build.version.release");
            if (string.IsNullOrWhiteSpace(android))
                android = "Unknown";

            var sdk = await GetProp(address, "ro.build.version.sdk");
            int.TryParse(sdk, out int _sdk);

            var arch = await GetProp(address, "ro.product.cpu.abi");

            OnProcess?.Invoke(false);
            OVERRIDE_ONPROCESSEVENT = false;

            return new Device()
            {
                Name = name,
                Android = android,
                Sdk = _sdk,
                Arch = arch
            };
        }
    }
}
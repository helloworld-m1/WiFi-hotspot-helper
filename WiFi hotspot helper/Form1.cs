using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace WiFi_hotspot_helper
{
    public partial class Form1 : Form
    {
        private bool _isLoading;
        private bool _isManaging;
        private Timer _saveDebounceTimer;

        private bool? _lastHotspotDesiredState;

        private bool? _lastObservedHotspotEnabled;
        private DateTime _lastHotspotStatusCheckUtc = DateTime.MinValue;

        private const string ConfigFileName = "config.ini";
        private const string RunRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunRegistryValueName = "WiFi_hotspot_helper";
        private const string DefaultExecMode = "netsh"; // netsh | mobile

        private sealed class AppConfig
        {
            /// <summary>
            /// 热点名称
            /// </summary>
            public string HotspotName { get; set; }
            /// <summary>
            /// 热点密码
            /// </summary>
            public string HotspotPassword { get; set; }
            /// <summary>
            /// 是否自动管理热点
            /// </summary>
            public bool AutoManage { get; set; }
            /// <summary>
            /// 绑定的网络适配器 ID （GUID）
            /// </summary>
            public string BoundAdapterId { get; set; }
            /// <summary>
            /// 执行模式
            /// </summary>
            public string ExecMode { get; set; }
        }

        private sealed class AdapterItem
        {
            public string Guid { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(Name) ? Guid : $"{Name} ({Guid})";
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeSaveDebounce();
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ensure any pending changes are saved before closing.
            if (_saveDebounceTimer.Enabled)
            {
                _saveDebounceTimer.Stop();
                SaveConfig();
            }
        }

        private void InitializeSaveDebounce()
        {
            _saveDebounceTimer = new Timer();
            _saveDebounceTimer.Interval = 500;
            _saveDebounceTimer.Tick += (s, e) =>
            {
                _saveDebounceTimer.Stop();

                if (_isLoading)
                {
                    return;
                }

                SaveConfig();
            };

            if (checkBox_autoManage != null)
            {
                checkBox_autoManage.CheckedChanged += (s, e) => MarkConfigDirty();
            }
        }

        /// <summary>
        /// 标记配置已更改，启动防抖保存计时器
        /// </summary>
        private void MarkConfigDirty()
        {
            if (_isLoading)
            {
                return;
            }

            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        private string GetConfigDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WiFi hotspot helper");
        }

        private string GetConfigPath()
        {
            return Path.Combine(GetConfigDirectory(), ConfigFileName);
        }

        private void AppendLog(string message)
        {
            if (textBox_log == null)
            {
                return;
            }

            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            if (textBox_log.InvokeRequired)
            {
                textBox_log.Invoke(new Action(() => textBox_log.AppendText(line + Environment.NewLine)));
            }
            else
            {
                textBox_log.AppendText(line + Environment.NewLine);
            }
        }

        private static string GetString(Dictionary<string, string> dict, string key)
        {
            if (dict.TryGetValue(key, out var v))
            {
                return v;
            }

            return string.Empty;
        }

        private static bool GetBool(Dictionary<string, string> dict, string key)
        {
            var s = GetString(dict, key);
            return s.Equals("1") || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        private Dictionary<string, string> ReadIniFromDisk()
        {
            var path = GetConfigPath();
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in File.ReadAllLines(path, Encoding.UTF8))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                var idx = line.IndexOf('=');
                if (idx <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, idx).Trim();
                var value = line.Substring(idx + 1).Trim();
                dict[key] = value;
            }

            return dict;
        }

        private void WriteIniToDisk(AppConfig config)
        {
            Directory.CreateDirectory(GetConfigDirectory());

            var path = GetConfigPath();
            var tmp = path + ".tmp";

            var sb = new StringBuilder();
            sb.AppendLine("; WiFi hotspot helper configuration");
            sb.AppendLine("; Encoding: UTF-8");
            sb.AppendLine($"SavedAt={DateTime.Now:O}");
            sb.AppendLine($"HotspotName={EscapeIni(config.HotspotName)}");
            sb.AppendLine($"HotspotPassword={EscapeIni(config.HotspotPassword)}");
            sb.AppendLine($"AutoManage={(config.AutoManage ? "1" : "0")}");
            sb.AppendLine($"BoundAdapterId={EscapeIni(config.BoundAdapterId)}");
            sb.AppendLine($"ExecMode={EscapeIni(config.ExecMode ?? DefaultExecMode)}");

            File.WriteAllText(tmp, sb.ToString(), Encoding.UTF8);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tmp, path);
        }

        private static string EscapeIni(string value)
        {
            value = value ?? string.Empty;
            return value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static string UnescapeIni(string value)
        {
            value = value ?? string.Empty;
            var sb = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c != '\\' || i == value.Length - 1)
                {
                    sb.Append(c);
                    continue;
                }

                var n = value[i + 1];
                if (n == 'n')
                {
                    sb.Append('\n');
                    i++;
                    continue;
                }

                if (n == 'r')
                {
                    sb.Append('\r');
                    i++;
                    continue;
                }

                if (n == '\\')
                {
                    sb.Append('\\');
                    i++;
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        private AppConfig GetConfigFromUi()
        {
            var mode = (radioButton_Mobile != null && radioButton_Mobile.Checked) ? "mobile" : "netsh";

            return new AppConfig
            {
                HotspotName = (hotspot_name.Text ?? string.Empty).Trim(),
                HotspotPassword = hotspot_password.Text ?? string.Empty,
                AutoManage = checkBox_autoManage.Checked,
                BoundAdapterId = (textBox1.Text ?? string.Empty).Trim(),
                ExecMode = mode
            };
        }

        private void ApplyConfigToUi(AppConfig config)
        {
            if (config == null)
            {
                return;
            }

            hotspot_name.Text = config.HotspotName ?? string.Empty;
            hotspot_password.Text = config.HotspotPassword ?? string.Empty;
            checkBox_autoManage.Checked = config.AutoManage;
            textBox1.Text = config.BoundAdapterId ?? string.Empty;

            // exec mode
            var mode = (config.ExecMode ?? DefaultExecMode).Trim();
            if (radioButton_Mobile != null && radioButton_netsh != null)
            {
                radioButton_Mobile.Checked = mode.Equals("mobile", StringComparison.OrdinalIgnoreCase);
                radioButton_netsh.Checked = !radioButton_Mobile.Checked;
            }
        }

        private List<AdapterItem> GetNetworkAdaptersForUi()
        {
            var result = new List<AdapterItem>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only show "real" candidates: Up/Down both allowed, but must have a GUID Id
                var id = ni.Id;
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                result.Add(new AdapterItem { Guid = id, Name = ni.Name });
            }

            return result
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Guid)
                .ToList();
        }

        private void RefreshAdapters()
        {
            try
            {
                var items = GetNetworkAdaptersForUi();

                comboBox1.BeginUpdate();
                comboBox1.Items.Clear();
                foreach (var item in items)
                {
                    comboBox1.Items.Add(item);
                }

                // If currently bound, try select it
                var boundGuid = (textBox1.Text ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(boundGuid))
                {
                    for (int i = 0; i < comboBox1.Items.Count; i++)
                    {
                        if (comboBox1.Items[i] is AdapterItem ai && string.Equals(ai.Guid, boundGuid, StringComparison.OrdinalIgnoreCase))
                        {
                            comboBox1.SelectedIndex = i;
                            break;
                        }
                    }
                }

                comboBox1.EndUpdate();
                AppendLog($"已刷新适配器列表：{items.Count} 个");
            }
            catch (Exception ex)
            {
                AppendLog("刷新适配器失败：" + ex.Message);
            }
        }

        private static bool IsApipa(System.Net.IPAddress ip)
        {
            var b = ip.GetAddressBytes();
            return b.Length == 4 && b[0] == 169 && b[1] == 254;
        }

        private bool TryGetAdapterIpv4(string adapterGuid, out string ipv4)
        {
            ipv4 = null;

            if (string.IsNullOrWhiteSpace(adapterGuid))
            {
                return false;
            }

            var normalizedGuid = NormalizeGuidString(adapterGuid);
            var ni = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => string.Equals(NormalizeGuidString(n.Id), normalizedGuid, StringComparison.OrdinalIgnoreCase));

            if (ni == null)
            {
                return false;
            }

            // Require adapter to be Up. Prevents stale IP after Wi-Fi disconnect.
            if (ni.OperationalStatus != OperationalStatus.Up)
            {
                return false;
            }

            var props = ni.GetIPProperties();

            // Require at least one IPv4 default gateway (routable). Avoids treating stale IP as connected.
            var hasIpv4Gateway = props.GatewayAddresses
                .Any(g => g?.Address != null && g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (!hasIpv4Gateway)
            {
                return false;
            }

            var ip = props.UnicastAddresses
                .Select(a => a.Address)
                .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IsApipa(a));

            if (ip == null)
            {
                return false;
            }

            ipv4 = ip.ToString();
            return true;
        }

        private bool HasIpv4Address(string adapterGuid)
        {
            return TryGetAdapterIpv4(adapterGuid, out _);
        }

        private void SetAutoRunEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryPath, writable: true))
            {
                if (key == null)
                {
                    throw new InvalidOperationException("无法打开注册表 Run 键");
                }

                if (enabled)
                {
                    var exePath = Application.ExecutablePath;
                    key.SetValue(RunRegistryValueName, "\"" + exePath + "\"");
                }
                else
                {
                    key.DeleteValue(RunRegistryValueName, throwOnMissingValue: false);
                }
            }
        }

        private bool GetAutoRunEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryPath, writable: false))
            {
                var v = key?.GetValue(RunRegistryValueName) as string;
                return !string.IsNullOrWhiteSpace(v);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshAdapters();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                // 从配置文件加载设置
                try
                {
                    var ini = ReadIniFromDisk();
                    var config = new AppConfig
                    {
                        HotspotName = UnescapeIni(GetString(ini, "HotspotName")),
                        HotspotPassword = UnescapeIni(GetString(ini, "HotspotPassword")),
                        AutoManage = GetBool(ini, "AutoManage"),
                        BoundAdapterId = UnescapeIni(GetString(ini, "BoundAdapterId")),
                        ExecMode = UnescapeIni(GetString(ini, "ExecMode"))
                    };

                    if (string.IsNullOrWhiteSpace(config.ExecMode))
                    {
                        config.ExecMode = DefaultExecMode;
                    }

                    ApplyConfigToUi(config);
                }
                catch (Exception ex)
                {
                    AppendLog("读取配置失败：" + ex.Message);
                }

                // 从注册表加载自动运行状态
                try
                {
                    checkBox_autoRun.Checked = GetAutoRunEnabled();
                }
                catch (Exception ex)
                {
                    AppendLog("读取开机自启状态失败：" + ex.Message);
                }

                RefreshAdapters();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private static string NormalizeGuidString(string guid)
        {
            if (guid == null)
            {
                return string.Empty;
            }

            return guid.Trim().Trim('{', '}');
        }

        private bool TrySetHotspotEnabled(bool enabled, out string error)
        {
            if (radioButton_Mobile != null && radioButton_Mobile.Checked)
            {
                return TrySetMobileHotspotEnabledWinrt(enabled, out error);
            }

            return TrySetHostedNetworkEnabled(enabled, out error);
        }

        private bool TrySetHostedNetworkEnabled(bool enabled, out string error)
        {
            // Existing netsh hostednetwork implementation.
            try
            {
                var config = GetConfigFromUi();
                if (enabled)
                {
                    if (string.IsNullOrWhiteSpace(config.HotspotName))
                    {
                        error = "热点名称为空";
                        return false;
                    }

                    // netsh hostednetwork requires WPA2-PSK key; empty password is not supported.
                    if (string.IsNullOrEmpty(config.HotspotPassword))
                    {
                        error = "netsh 模式不支持无密码热点，请切换到 移动热点(Mobile) 模式或设置 8-63 位密码";
                        return false;
                    }

                    if (config.HotspotPassword.Length < 8 || config.HotspotPassword.Length > 63)
                    {
                        error = "热点密码长度必须在 8 到 63 个字符之间";
                        return false;
                    }

                    var setArgs = $"wlan set hostednetwork mode=allow ssid=\"{config.HotspotName.Replace("\"", "") }\" key=\"{config.HotspotPassword.Replace("\"", "") }\"";
                    if (!TryRunNetsh(setArgs, out var setErr))
                    {
                        error = setErr;
                        return false;
                    }

                    if (!TryRunNetsh("wlan start hostednetwork", out var startErr))
                    {
                        var hint = BuildHostedNetworkNotSupportedHint(startErr);
                        error = string.IsNullOrWhiteSpace(hint) ? startErr : (startErr + Environment.NewLine + hint);
                        return false;
                    }

                    error = null;
                    return true;
                }

                if (!TryRunNetsh("wlan stop hostednetwork", out var stopErr))
                {
                    error = stopErr;
                    return false;
                }

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private bool TrySetMobileHotspotEnabledWinrt(bool enabled, out string error)
        {
            try
            {
                var bound = (textBox1.Text ?? string.Empty).Trim();
                bound = NormalizeGuidString(bound);

                var config = GetConfigFromUi();
                var ssid = (config.HotspotName ?? string.Empty).Replace("\"", "").Replace("'", "''");
                var key = (config.HotspotPassword ?? string.Empty).Replace("\"", "").Replace("'", "''");

                var ps =
                    "$ErrorActionPreference='Stop';" +
                    "Add-Type -AssemblyName System.Runtime.WindowsRuntime;" +
                    "$t=[Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager,Windows.Networking.NetworkOperators,ContentType=WindowsRuntime];" +
                    "$targetGuid='{GUID}';" +
                    "$ssid='{SSID}';" +
                    "$key='{KEY}';" +
                    "$p=[Windows.Networking.Connectivity.NetworkInformation]::GetInternetConnectionProfile();" +
                    "if($p -eq $null -or $p.NetworkAdapter -eq $null -or ($targetGuid -ne '' -and $p.NetworkAdapter.NetworkAdapterId.ToString() -ine $targetGuid)){" +
                    "  $profiles=[Windows.Networking.Connectivity.NetworkInformation]::GetConnectionProfiles();" +
                    "  if($targetGuid -ne ''){ $p=$profiles | Where-Object { $_.NetworkAdapter -ne $null -and $_.NetworkAdapter.NetworkAdapterId.ToString() -ieq $targetGuid } | Select-Object -First 1 }" +
                    "  if($p -eq $null){ $p=$profiles | Select-Object -First 1 }" +
                    "}" +
                    "if($p -eq $null){ throw 'No connection profile.' };" +
                    "$m=$t::CreateFromConnectionProfile($p);" +
                    "$state0=[int]$m.TetheringOperationalState;" +
                    (enabled ? "" : "if($state0 -eq 0){ return };" ) +
                    // Configure SSID always when enabling. Configure passphrase only when non-empty.
                    (enabled
                        ? "try { $c=$m.GetCurrentAccessPointConfiguration(); if($ssid -ne ''){ $c.Ssid=$ssid }; if($key -ne ''){ $c.Passphrase=$key }; $null=$m.ConfigureAccessPointAsync($c); } catch { };"
                        : "") +
                    (enabled ? "$null=$m.StartTetheringAsync();" : "$null=$m.StopTetheringAsync();") +
                    "$sw=[Diagnostics.Stopwatch]::StartNew();" +
                    "while($sw.ElapsedMilliseconds -lt 30000){" +
                    "  $state=[int]$m.TetheringOperationalState;" +
                    (enabled
                        ? "  if($state -eq 1){ return }"
                        : "  if($state -eq 0){ return }") +
                    "  Start-Sleep -Milliseconds 300;" +
                    "}" +
                    "$state=[int]$m.TetheringOperationalState;" +
                    (enabled
                        ? "throw ('Tethering start timed out. OperationalState=' + $state)"
                        : "throw ('Tethering stop timed out. OperationalState=' + $state)");

                ps = ps.Replace("{GUID}", bound.Replace("'", "''"));
                ps = ps.Replace("{SSID}", ssid);
                ps = ps.Replace("{KEY}", key);

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + ps.Replace("\"", "`\"") + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (var p = Process.Start(psi))
                {
                    var stdout = (p.StandardOutput.ReadToEnd() ?? string.Empty).Replace("\0", string.Empty);
                    var stderr = (p.StandardError.ReadToEnd() ?? string.Empty).Replace("\0", string.Empty);
                    p.WaitForExit(35000);

                    if (p.ExitCode != 0)
                    {
                        error = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                        return false;
                    }
                }

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool LooksGarbled(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            // Heuristic: replacement char or lots of high chars but no common CJK punctuation/words.
            if (s.IndexOf('\uFFFD') >= 0)
            {
                return true;
            }

            // If contains many characters from the "CJK Compatibility Forms" ranges-like garbage is hard;
            // just detect absence of typical Chinese markers while having non-ASCII.
            var hasNonAscii = s.Any(ch => ch > 127);
            var hasCjk = s.Any(ch => ch >= 0x4E00 && ch <= 0x9FFF);
            return hasNonAscii && !hasCjk;
        }

        private bool TryRunNetsh(string arguments, out string error)
        {
            AppendLog($"正在执行: netsh {arguments}");

            try
            {
                // Prefer UTF-8 (65001) as user environment shows correct output under it.
                var utf8 = Encoding.UTF8;
                var gbk = Encoding.GetEncoding(936);

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c chcp 65001>nul & netsh.exe " + arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = utf8,
                    StandardErrorEncoding = utf8
                };

                using (var p = Process.Start(psi))
                {
                    var stdout = p.StandardOutput.ReadToEnd() ?? string.Empty;
                    var stderr = p.StandardError.ReadToEnd() ?? string.Empty;
                    p.WaitForExit(15000);

                    // Fallback: If output looks garbled under UTF-8, re-run once with GBK.
                    if (LooksGarbled(stdout) || LooksGarbled(stderr))
                    {
                        var psi2 = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c chcp 936>nul & netsh.exe " + arguments,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            StandardOutputEncoding = gbk,
                            StandardErrorEncoding = gbk
                        };

                        using (var p2 = Process.Start(psi2))
                        {
                            stdout = p2.StandardOutput.ReadToEnd() ?? string.Empty;
                            stderr = p2.StandardError.ReadToEnd() ?? string.Empty;
                            p2.WaitForExit(15000);

                            // keep original exit code behavior consistent
                            if (p2.ExitCode != 0)
                            {
                                var all2 = (stdout + "\n" + stderr).Trim();
                                error = string.IsNullOrWhiteSpace(all2) ? "netsh 执行失败" : all2;
                                return false;
                            }
                        }
                    }

                    var all = (stdout + "\n" + stderr).Trim();

                    if (p.ExitCode != 0)
                    {
                        error = string.IsNullOrWhiteSpace(all) ? "netsh 执行失败" : all;
                        return false;
                    }

                    var looksFailed =
                        all.IndexOf("未能", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        all.IndexOf("失败", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        all.IndexOf("couldn't be started", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        all.IndexOf("not in the correct state", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (all.IndexOf("hosted network", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         all.IndexOf("not available", StringComparison.OrdinalIgnoreCase) >= 0) ||
                        all.IndexOf("不可用", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (looksFailed)
                    {
                        error = string.IsNullOrWhiteSpace(all) ? "netsh 执行失败" : all;
                        return false;
                    }

                    error = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string BuildHostedNetworkNotSupportedHint(string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
            {
                return "";
            }

            // Common when Hosted Network is disabled/unsupported on Win10/11 drivers.
            if (rawError.IndexOf("not in the correct state", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rawError.IndexOf("组或资源的状态不是", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rawError.IndexOf("not available", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rawError.IndexOf("不可用", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "提示：该系统/无线网卡驱动可能不支持 Hosted Network （netsh hostednetwork）。Win10/11 很多机器已废弃该能力，需要改用系统“移动热点(Mobile Hotspot)”方式。";
            }

            return "";
        }

        private bool _hotspotOpInFlight;
        private bool? _pendingHotspotEnable;
        private string _pendingHotspotReason;
        private string _lastLoggedBoundIpv4;

        private void RunHotspotOpAsync(bool enable, string reason)
        {
            // If an op is already running, remember the latest desired state.
            // "Stop" should override any earlier "start".
            if (_hotspotOpInFlight)
            {
                _pendingHotspotEnable = enable;
                _pendingHotspotReason = reason;
                return;
            }

            _hotspotOpInFlight = true;

            Task.Run(() =>
            {
                try
                {
                    AppendLog((enable ? "正在后台启动热点" : "正在后台停止热点") + (string.IsNullOrWhiteSpace(reason) ? "" : "：" + reason));

                    if (!TrySetHotspotEnabled(enable, out var err) && !string.IsNullOrWhiteSpace(err))
                    {
                        AppendLog((enable ? "启动热点失败：" : "停止热点失败：") + err);
                    }
                }
                finally
                {
                    _hotspotOpInFlight = false;

                    // Apply any queued request (latest wins)
                    if (_pendingHotspotEnable.HasValue)
                    {
                        var nextEnable = _pendingHotspotEnable.Value;
                        var nextReason = _pendingHotspotReason;
                        _pendingHotspotEnable = null;
                        _pendingHotspotReason = null;

                        RunHotspotOpAsync(nextEnable, nextReason);
                    }
                }
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!checkBox_autoManage.Checked)
            {
                return;
            }

            if (_isManaging)
            {
                return;
            }

            _isManaging = true;
            try
            {
                var boundGuid = (textBox1.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(boundGuid))
                {
                    _lastHotspotDesiredState = null;
                    _lastObservedHotspotEnabled = null;
                    AppendLog("自动管理：未绑定适配器");
                    return;
                }

                var config = GetConfigFromUi();
                if (string.IsNullOrWhiteSpace(config.HotspotName))
                {
                    _lastHotspotDesiredState = null;
                    _lastObservedHotspotEnabled = null;
                    AppendLog("自动管理：热点名称为空，跳过");
                    return;
                }

                var hasIp = TryGetAdapterIpv4(boundGuid, out var ipv4);

                // Reduce log spam: only log when IPv4 status/value changes.
                var ipLogValue = hasIp ? ipv4 : "";
                if (!string.Equals(_lastLoggedBoundIpv4 ?? string.Empty, ipLogValue ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    _lastLoggedBoundIpv4 = ipLogValue;
                    AppendLog(hasIp
                        ? $"自动管理：绑定适配器 IPv4={ipv4}"
                        : "自动管理：绑定适配器无有效 IPv4");
                }

                var desired = hasIp;

                // Periodically verify actual hotspot status so we can recover from external/system stop.
                // (e.g. Mobile Hotspot can auto-stop when no clients are connected)
                var nowUtc = DateTime.UtcNow;
                var shouldCheckStatus = _lastHotspotStatusCheckUtc == DateTime.MinValue || (nowUtc - _lastHotspotStatusCheckUtc).TotalSeconds >= 10;
                if (shouldCheckStatus)
                {
                    _lastHotspotStatusCheckUtc = nowUtc;

                    if (TryGetHotspotEnabled(out var isOn, out var statusErr))
                    {
                        if (!_lastObservedHotspotEnabled.HasValue || _lastObservedHotspotEnabled.Value != isOn)
                        {
                            _lastObservedHotspotEnabled = isOn;
                            AppendLog("当前热点状态：" + (isOn ? "已开启" : "未开启"));
                        }

                        // If we want it ON but it got turned OFF externally, restart it.
                        if (desired && !isOn)
                        {
                            AppendLog("自动管理：检测到热点被关闭，尝试重新开启");
                            RunHotspotOpAsync(true, "自动恢复");
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(statusErr))
                    {
                        AppendLog("读取热点状态失败：" + statusErr);
                    }
                }

                // Only send enable/disable when desired state changes.
                if (_lastHotspotDesiredState.HasValue && _lastHotspotDesiredState.Value == desired)
                {
                    return;
                }

                _lastHotspotDesiredState = desired;

                if (!hasIp)
                {
                    AppendLog("自动管理：绑定适配器未获得 IPv4，停止热点");
                    RunHotspotOpAsync(false, "自动管理");
                    return;
                }

                AppendLog("自动管理：绑定适配器已获得 IPv4，启动热点");
                RunHotspotOpAsync(true, "自动管理");
            }
            finally
            {
                _isManaging = false;
            }
        }

        private void SaveConfig()
        {
            var config = GetConfigFromUi();

            if (config.HotspotName.Length == 0)
            {
                AppendLog("未保存：热点名称为空");
                return;
            }

            if (config.HotspotPassword.Length > 0 && (config.HotspotPassword.Length < 8 || config.HotspotPassword.Length > 63))
            {
                AppendLog("未保存：热点密码长度必须在 8 到 63 个字符之间");
                return;
            }
            try
            {
                WriteIniToDisk(config);
                AppendLog("配置已保存：" + GetConfigPath());

                // If hotspot was previously expected to be ON, stop it in background so new settings take effect.
                if (config.AutoManage)
                {
                    var wasOn = _lastHotspotDesiredState.HasValue && _lastHotspotDesiredState.Value;
                    if (wasOn)
                    {
                        AppendLog("配置已更新：为应用新设置，先停止热点，等待下一次自动管理再启动");
                        RunHotspotOpAsync(false, "配置更新");
                    }

                    _lastHotspotDesiredState = null;
                }
            }
            catch (Exception ex)
            {
                AppendLog("保存配置失败：" + ex.Message);
            }
        }

        private void hotspot_name_TextChanged(object sender, EventArgs e)
        {
            MarkConfigDirty();
        }

        private void hotspot_password_TextChanged(object sender, EventArgs e)
        {
            MarkConfigDirty();
        }

        private void checkBox_autoRun_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            try
            {
                SetAutoRunEnabled(checkBox_autoRun.Checked);
                AppendLog(checkBox_autoRun.Checked ? "已启用开机自启" : "已关闭开机自启");
            }
            catch (Exception ex)
            {
                AppendLog("设置开机自启失败：" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem is AdapterItem ai)
            {
                textBox1.Text = ai.Guid;
                AppendLog("已绑定适配器：" + ai);
                SaveConfig();
                return;
            }

            AppendLog("绑定失败：请先选择一个适配器");
        }

        private void radioButton_netsh_CheckedChanged(object sender, EventArgs e)
        {
            MarkConfigDirty();
        }

        private void radioButton_Mobile_CheckedChanged(object sender, EventArgs e)
        {
            MarkConfigDirty();
        }

        private bool TryGetHotspotEnabled(out bool enabled, out string error)
        {
            enabled = false;

            if (radioButton_Mobile != null && radioButton_Mobile.Checked)
            {
                return TryGetMobileHotspotEnabledWinrt(out enabled, out error);
            }

            return TryGetHostedNetworkEnabled(out enabled, out error);
        }

        private bool TryGetHostedNetworkEnabled(out bool enabled, out string error)
        {
            enabled = false;
            error = null;

            try
            {
                // "Hosted network status" -> Started/Stopped
                if (!TryRunNetsh("wlan show hostednetwork", out var output))
                {
                    error = output;
                    return false;
                }

                var s = output ?? string.Empty;

                var started =
                    s.IndexOf("Status", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (s.IndexOf("Started", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("已启动", StringComparison.OrdinalIgnoreCase) >= 0);

                var stopped =
                    s.IndexOf("Status", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (s.IndexOf("Stopped", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("已停止", StringComparison.OrdinalIgnoreCase) >= 0);

                if (started)
                {
                    enabled = true;
                    return true;
                }

                if (stopped)
                {
                    enabled = false;
                    return true;
                }

                // Fallback heuristic
                enabled = s.IndexOf("Started", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("已启动", StringComparison.OrdinalIgnoreCase) >= 0;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private bool TryGetMobileHotspotEnabledWinrt(out bool enabled, out string error)
        {
            enabled = false;

            try
            {
                var bound = (textBox1.Text ?? string.Empty).Trim();
                bound = NormalizeGuidString(bound);

                var ps =
                    "$ErrorActionPreference='Stop';" +
                    "Add-Type -AssemblyName System.Runtime.WindowsRuntime;" +
                    "$t=[Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager,Windows.Networking.NetworkOperators,ContentType=WindowsRuntime];" +
                    "$targetGuid='{GUID}';" +
                    "$p=[Windows.Networking.Connectivity.NetworkInformation]::GetInternetConnectionProfile();" +
                    "if($p -eq $null -or $p.NetworkAdapter -eq $null -or ($targetGuid -ne '' -and $p.NetworkAdapter.NetworkAdapterId.ToString() -ine $targetGuid)){" +
                    "  $profiles=[Windows.Networking.Connectivity.NetworkInformation]::GetConnectionProfiles();" +
                    "  if($targetGuid -ne ''){ $p=$profiles | Where-Object { $_.NetworkAdapter -ne $null -and $_.NetworkAdapter.NetworkAdapterId.ToString() -ieq $targetGuid } | Select-Object -First 1 }" +
                    "  if($p -eq $null){ $p=$profiles | Select-Object -First 1 }" +
                    "}" +
                    "if($p -eq $null){ throw 'No connection profile.' };" +
                    "$m=$t::CreateFromConnectionProfile($p);" +
                    // output state as int
                    "[int]$m.TetheringOperationalState";

                ps = ps.Replace("{GUID}", bound.Replace("'", "''"));

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + ps.Replace("\"", "`\"") + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (var p = Process.Start(psi))
                {
                    var stdout = (p.StandardOutput.ReadToEnd() ?? string.Empty).Replace("\0", string.Empty).Trim();
                    var stderr = (p.StandardError.ReadToEnd() ?? string.Empty).Replace("\0", string.Empty).Trim();
                    p.WaitForExit(15000);

                    if (p.ExitCode != 0)
                    {
                        error = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                        return false;
                    }

                    if (!int.TryParse(stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty, out var state))
                    {
                        error = "无法解析移动热点状态：" + stdout;
                        return false;
                    }

                    // 0 Off, 1 On, 2 InTransition, 3 Unknown
                    enabled = state == 1;
                    error = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}

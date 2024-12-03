using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AssocManager.OSTools
{
    internal class Windows
    {
        public static string GetApplicationDir() => Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        public static string GetDesktopDir() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static void CreateShortcutWithPowerShell(string shortcutPath, string targetFilePath, string iconFilePath)
        {
            // PowerShell 脚本来创建快捷方式
            string script = $@"
$WshShell = New-Object -ComObject WScript.Shell
$shortcut = $WshShell.CreateShortcut('{shortcutPath}')
$shortcut.TargetPath = '{targetFilePath}'
$shortcut.IconLocation = '{iconFilePath}'
$shortcut.Save()";

            // 创建 PowerShell 进程
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            // 启动 PowerShell 进程并执行脚本
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit(); // 等待进程执行完毕
            }
        }
        public static bool CreateIcon(string baseDir,string filePath,string icon,string Name)
        {
            string file = Path.Combine(baseDir, Name + ".lnk");
            if (File.Exists(file)) return true;

            CreateShortcutWithPowerShell(file, filePath, icon);

            return true;
        }




        public static bool SetFileAssociation(string fileExtension, string progID,string desc, string applicationPath, string iconPath)
        {
            if ((!File.Exists(applicationPath))) return false;
            using (RegistryKey userRoot = Registry.CurrentUser.OpenSubKey("Software\\Classes",true))
            {

                // 创建注册表项并关联文件扩展名到 ProgID
                using (RegistryKey key = userRoot.CreateSubKey(fileExtension))
                {
                    if (key != null)
                    {
                        key.SetValue("", progID);  // 关联文件扩展名到 ProgID
                    }
                }

                // 设置文件类型的描述（可选）
                using (RegistryKey key = userRoot.CreateSubKey(progID))
                {
                    if (key != null)
                    {
                        key.SetValue("", desc);  // 设置文件类型的描述
                    }
                }

                // 设置文件类型的图标
                using (RegistryKey key = userRoot.CreateSubKey($@"{progID}\DefaultIcon"))
                {
                    if (key != null)
                    {
                        key.SetValue("", iconPath);  // 设置文件类型的图标路径
                    }
                }

                // 设置文件类型的打开方式
                using (RegistryKey key = userRoot.CreateSubKey($@"{progID}\shell\open\command"))
                {
                    if (key != null)
                    {
                        key.SetValue("", $"\"{applicationPath}\" \"%1\"");  // 设置打开文件的程序路径
                    }
                }
            }
            return true;
        }
    }
}

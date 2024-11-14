using ExtensionManager.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TuneLab.Extensions;

namespace ExtensionManager
{
    internal static class Worker
    {
        public static int ExtensionSelectedIndex = -1;
        public static List<string> ExtensionList = new List<string>() { };

        public static bool isWorking = false;

        public static string GuiTitle = "TuneLab Extension Manager";

        public static void UpdateExtensions()
        {
            string oldName = "";
            if (ExtensionList.Count > ExtensionSelectedIndex && ExtensionSelectedIndex >= 0) oldName = ExtensionList[ExtensionSelectedIndex];
            ExtensionList.Clear();
            ExtensionLists.LoadExtensions();
            ExtensionList.AddRange(ExtensionLists.Extensions.Select(p => String.Format("{0} ({1})",p.name,p.version)).ToList());
            ExtensionSelectedIndex = Array.FindIndex(ExtensionList.ToArray(), item => item == oldName);
        }

        public static bool IsHaveSetting(object index)
        {
            if (typeof(int) != index.GetType()) return false;
            if((int)index<0 || (int)index>=ExtensionList.Count) return false;   
            string dir = Path.Combine(ExtensionLists.ExtensionPaths[(int)index],"setting_ui");
            if (Path.Exists(dir))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (Path.Exists(Path.Combine(dir, "setting.exe"))) return true;
                }
                else
                {
                    if (Path.Exists(Path.Combine(dir, "setting.sh"))) return true;
                }
            }
            return false;
        }
        public static void StartSetting(object index)
        {
            if(!IsHaveSetting(index)) return;
            string dir = Path.Combine(ExtensionLists.ExtensionPaths[(int)index], "setting_ui");
            if (Path.Exists(dir))
            {
                string binFile = Path.Combine(dir, "setting");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) binFile = binFile + ".exe";
                else
                {
                    binFile = binFile + ".sh";
                    {
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"a+x \"" + binFile + "\"",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (Process process = new Process { StartInfo = psi })
                            {
                                // 启动进程
                                process.Start();
                                // 等待命令执行完成
                                process.WaitForExit();
                            }
                        }
                        catch {; }
                    }
                }
                Process.Start(binFile);
            }
        }

        public static bool CheckTuneLabAlive()
        {
            string lockFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TuneLab", "TuneLab.lock");
            return File.Exists(lockFilePath);
        }
        public static void UnlockTuneLabAlive()
        {
            string lockFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TuneLab", "TuneLab.lock");
            if(File.Exists(lockFilePath))File.Delete(lockFilePath);
        }
        public static void InstallExtension()
        {
            var ret=NativeFileDialogSharp.Dialog.FileOpen("tlx");
            if (ret.IsOk)
            {
                if (Path.Exists(ret.Path))
                {
                    Task.Run(() =>
                    {
                        isWorking = true;
                        try
                        {
                            bool restart = false;
                            var extensionFolder = ExtensionLists.ExtensionsFolder;

                            var name = Path.GetFileNameWithoutExtension(ret.Path);
                            var entry = ZipFile.OpenRead(ret.Path).GetEntry("description.json");
                            if (entry != null)
                            {
                                var description = JsonSerializer.Deserialize<ExtensionDescription>(entry.Open());
                                if (!string.IsNullOrEmpty(description.name))
                                    name = description.name;
                            }
                            var dir = Path.Combine(extensionFolder, name);
                            Console.WriteLine("Uninstalling " + name + "...");
                            if (Directory.Exists(dir))
                            {
                                while (true)
                                {
                                    try
                                    {
                                        Directory.Delete(dir, true);
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Failed to delete file: " + ex.ToString());
                                        Console.WriteLine("Try again...");
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                            Console.WriteLine("Installing " + name + "...");
                            ZipFileHelper.ExtractToDirectory(ret.Path, dir);
                            Console.WriteLine(name + " has been successfully installed!\n");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Installation failed: " + ex.ToString());
                        }
                        UpdateExtensions();
                        isWorking = false;
                    });
                }
            }
        }
        public static void UninstallExtension(object index)
        {
            if (typeof(int) != index.GetType()) return;

            string dir = ExtensionLists.ExtensionPaths[(int)index];
            if (Path.Exists(dir))
            {
                Task.Run(() =>
                {
                    isWorking = true;
                    try
                    {
                        var k = File.OpenRead(Path.Combine(dir, "description.json"));
                        var description = JsonSerializer.Deserialize<ExtensionDescription>(k);
                        k.Close();
                        if (description != null)
                        {
                            if (description.name == ExtensionLists.Extensions[(int)index].name)
                            {
                                while (true)
                                {
                                    try
                                    {
                                        Directory.Delete(dir, true);
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Failed to delete file: " + ex.ToString());
                                        Console.WriteLine("Try again...");
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                    }
                    catch {; }
                    UpdateExtensions();
                    isWorking = false;
                });
            }
        }
    }
}

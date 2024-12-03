using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AssocManager
{
    internal static class Worker
    {
        public static int ExtensionSelectedIndex = -1;
        public static List<string> ExtensionList = new List<string>() { };



        public static bool isWorking = false;
        public static string GuiTitle = "TuneLab Assoc Manager";
        public static string TuneLabPath = "";
        public static string Version = "";
        public static bool SupportCmdOpen = true;

        public static void LoadPath()
        {
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string setFile = Path.Combine(UserProfile, ".TuneLab", "paths.txt");
            if (File.Exists(setFile))
            {
                var list = File.ReadAllLines(setFile);
                if(list!=null && list.Length > 0)
                {
                    TuneLabPath = list[0];
                }
            }
        }
        public static void SavePath()
        {
            string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string setFile = Path.Combine(UserProfile, ".TuneLab", "paths.txt");
            string[] list = new string[1];
            if(File.Exists(setFile))list=File.ReadAllLines(setFile);
            if (list.Length < 1) list = new string[1];
            list[0] = TuneLabPath;
            File.WriteAllLines(setFile,list);
        }
        public static void UpdateInfo()
        {
            if (File.Exists(Path.Combine(TuneLabPath, "TuneLab.dll")))
            {
                FileVersionInfo fv = FileVersionInfo.GetVersionInfo(Path.Combine(TuneLabPath, "TuneLab.dll"));
                Version=fv.FileVersion!=null?fv.FileVersion:"";
                try
                {
                    string[] x = Version.Split('.');
                    if(int.TryParse(x[0], out int maj) &&
                        int.TryParse(x[1], out int mio) &&
                        int.TryParse(x[2],out int bud))
                    {
                        if (maj > 1) SupportCmdOpen = true;
                        else if (maj == 1)
                        {
                            if (mio > 5) SupportCmdOpen = true;
                            else if (mio == 5)
                            {
                                if (bud > 8) SupportCmdOpen = true;
                                else SupportCmdOpen = false;
                            }
                            else SupportCmdOpen = false;
                        }
                        else SupportCmdOpen = false;
                    }
                }
                catch {; }
            }else
            {
                Version = "<Not Found>";
            }
        }

        public static string InstallIcon(string fileName)
        {
          //  try
            {
                string ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".ico" : ".png";
                string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string IconDir = Path.Combine(UserProfile,".TuneLab", "Icons");
                if (!Directory.Exists(IconDir)) { Directory.CreateDirectory(IconDir); }
                string tg = Path.Combine(IconDir, fileName + ext);
                if (File.Exists(tg)) return tg;

                string dllPath = Assembly.GetExecutingAssembly().Location;
                string dllDir = Path.GetDirectoryName(dllPath);
                string assetDir = Path.Combine(dllDir, "Icons");
                string sg = Path.Combine(assetDir, fileName + ext);
                File.Copy(sg, tg, true);
                if (File.Exists(tg)) return tg;
            }
            //catch {; }
            return "";
        }
        public static bool CreateIcon(int Type)
        {
            string desktopPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSTools.Windows.GetDesktopDir() : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSTools.Linux.GetDesktopDir() : "";
            string startPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSTools.Windows.GetApplicationDir() : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSTools.Linux.GetApplicationDir() : "";
            string bDir = Type==2?startPath:desktopPath;
            string icon =InstallIcon("editor");
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSTools.Windows.CreateIcon(bDir,Path.Combine(TuneLabPath, "TuneLab.exe"), icon, "TuneLab");
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSTools.Linux.CreateIcon(bDir, Path.Combine(TuneLabPath, "TuneLab"), icon, "TuneLab", "application/vnd.tunelab.project");
            }
            return false;
        }
        public static bool SetAlloc(int Type)
        {
            string tlxicon = InstallIcon("tlx");
            string tlpicon = InstallIcon("tlp");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                bool ret = true;
                ret = ret && OSTools.Windows.SetFileAssociation(".tlp", "application/vnd.tunelab.project", "TuneLab Extension",Path.Combine(TuneLabPath, "TuneLab.exe"), tlpicon);
                ret = ret && OSTools.Windows.SetFileAssociation(".tlx", "application/vnd.tunelab.extension", "TuneLab Extension", Path.Combine(TuneLabPath, "ExtensionInstaller.exe"), tlxicon);
                return ret;
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string icon = InstallIcon("editor");
                string bDir = OSTools.Linux.GetApplicationDir();
                OSTools.Linux.CreateIcon(bDir, Path.Combine(TuneLabPath, "TuneLab"), icon, "TuneLab", "application/vnd.tunelab.project");
                OSTools.Linux.CreateIcon(bDir, Path.Combine(TuneLabPath, "ExtensionInstaller"), tlxicon, "TuneLab ExtensionInstaller", "application/vnd.tunelab.extension");
                OSTools.Linux.RegisterMimeType("tlp", "application/vnd.tunelab.project",tlpicon);
                OSTools.Linux.RegisterMimeType("tlx", "application/vnd.tunelab.extension",tlxicon);
                OSTools.Linux.SetDefaultApplicationForMimeType("tlp", "application/vnd.tunelab.project", "TuneLab");
                OSTools.Linux.SetDefaultApplicationForMimeType("tlx", "application/vnd.tunelab.extension", "TuneLab ExtensionInstaller");
                OSTools.Linux.UpdateMimeDatabase();
            }
            return false;
        }
    }
}

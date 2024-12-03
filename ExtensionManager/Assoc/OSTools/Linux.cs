using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssocManager.OSTools
{
    internal class Linux
    {
        public static string GetApplicationDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "applications");
        public static string GetDesktopDir() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static void CreateShortcut(string appName,string desktopFilePath, string execPath, string iconPath,string mime)
        {
            // 定义 .desktop 文件的内容
            string desktopFileContent = $@"[Desktop Entry]
Version=1.0
Name={appName}
Comment=Launch {appName}
Exec={execPath} %f
Icon={iconPath}
Terminal=false
Type=Application
Categories=Utility;Audio;
MimeType={mime};
";

            // 写入 .desktop 文件
            File.WriteAllText(desktopFilePath, desktopFileContent);

            // 为 .desktop 文件设置执行权限
            var fileInfo = new FileInfo(desktopFilePath);
            fileInfo.IsReadOnly = false;
            var chmod = new System.Diagnostics.ProcessStartInfo("chmod", $"+x {desktopFilePath}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            System.Diagnostics.Process.Start(chmod);

            Console.WriteLine($"Shortcut created at {desktopFilePath}");
        }
        public static bool CreateIcon(string baseDir, string filePath, string icon, string appName,string mime="")
        {
            string file = Path.Combine(baseDir, appName + ".desktop");
            if (File.Exists(file)) return true;
            string bDir = Path.GetDirectoryName(file);
            if (!Directory.Exists(bDir)) Directory.CreateDirectory(bDir);
            CreateShortcut(appName, file, filePath, icon, mime);
            return true;
        }




        public static bool SetFileAssociation(string fileExtension, string progID, string desc, string applicationPath, string iconPath) {
            return true;
        }
        public static void RegisterMimeType(string fileExtension, string progID, string iconPath)
        {
            // 2. 为文件扩展名注册 MIME 类型
            string mimeInfoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "mime", "packages", "user-mime-info.xml");

            // 创建 MIME 目录（如果不存在）
            Directory.CreateDirectory(Path.GetDirectoryName(mimeInfoPath));

            // MIME 类型内容
            string mimeTypeItem = $@"<mime-type type=""{progID}"">
    <glob pattern=""*.{fileExtension}""/>
    <icon name=""{iconPath}""/>
  </mime-type>";

            string mimeTypeContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<mime-info xmlns=""http://www.freedesktop.org/standards/shared-mime-info"">
  {mimeTypeItem}
</mime-info>";

            // 检查是否已存在 MIME 类型文件
            if (File.Exists(mimeInfoPath))
            {
                // 读取原有的 MIME 类型文件内容
                string existingContent = File.ReadAllText(mimeInfoPath);

                // 检查是否已注册该 MIME 类型
                if (!existingContent.Contains($"{progID}"))
                {
                    // 如果没有注册该 MIME 类型，追加到文件中
                    string updatedContent = existingContent.Replace("</mime-info>", mimeTypeItem + "\n</mime-info>");
                    File.WriteAllText(mimeInfoPath, updatedContent);

                    Console.WriteLine($"MIME type for {fileExtension} files registered with ProgID: {progID}");
                }
                else
                {
                    Console.WriteLine($"MIME type for {fileExtension} files is already registered.");
                }
            }
            else
            {
                // 如果 MIME 文件不存在，则直接创建并写入 MIME 类型内容
                File.WriteAllText(mimeInfoPath, mimeTypeContent);
                Console.WriteLine($"MIME type for {fileExtension} files registered with ProgID: {progID}");
            }
        }

        public static void UpdateMimeDatabase()
        {
            // 更新 MIME 数据库
            var updateMime = new System.Diagnostics.ProcessStartInfo("update-mime-database", "~/.local/share/mime")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            System.Diagnostics.Process.Start(updateMime);
        }

        public static void SetDefaultApplicationForMimeType(string fileExtension, string progID, string desktopFileName)
        {
            // 3. 修改 mimeapps.list 设置默认应用程序
            string mimeAppsListPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "applications", "mimeapps.list");

            // 创建 mimeapps.list 文件（如果不存在）
            if (!File.Exists(mimeAppsListPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(mimeAppsListPath));
                File.WriteAllText(mimeAppsListPath, "[Default Applications]\n");
            }

            // 读取 mimeapps.list 文件内容
            string mimeAppsListContent = File.ReadAllText(mimeAppsListPath);

            // MIME 类型
            string mimeType = $"{progID}";

            // 设置默认应用程序
            string defaultAppSetting = $"{mimeType}={desktopFileName}.desktop";

            // 如果 mimeapps.list 文件中没有此 MIME 类型的默认应用程序设置，添加它
            if (!mimeAppsListContent.Contains(defaultAppSetting))
            {
                mimeAppsListContent += $"{defaultAppSetting}\n";
                File.WriteAllText(mimeAppsListPath, mimeAppsListContent);
                Console.WriteLine($"Default application set for {fileExtension} files: {desktopFileName}.desktop");
            }
            else
            {
                Console.WriteLine($"Default application for {fileExtension} files is already set.");
            }
        }
    }
}

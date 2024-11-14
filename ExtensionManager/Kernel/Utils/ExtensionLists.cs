using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TuneLab.Extensions;

namespace ExtensionManager.Utils
{
    internal class ExtensionLists
    {
        static string AppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        static string TuneLabFolder => Path.Combine(AppDataFolder, "TuneLab");
        public static string ExtensionsFolder => Path.Combine(TuneLabFolder, "Extensions");
        public static void LoadExtensions()
        {
            Extensions.Clear();
            ExtensionPaths.Clear();
            foreach (var dir in Directory.GetDirectories(ExtensionsFolder))
            {
                Load(dir);
            }
        }
        public static List<ExtensionDescription> Extensions { get; private set; } = new List<ExtensionDescription>();
        public static List<string> ExtensionPaths { get; private set; } = new List<string>();
        static void Load(string path)
        {
            string descriptionPath = Path.Combine(path, "description.json");
            var extensionName = Path.GetFileName(path);
            ExtensionDescription? description = null;
            if (File.Exists(descriptionPath))
            {
                try
                {
                    var k = File.OpenRead(descriptionPath);
                    description = JsonSerializer.Deserialize<ExtensionDescription>(k);
                    k.Close();
                    if (description != null)
                    {
                        Extensions.Add(description);
                        ExtensionPaths.Add(path);
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }
    }
}

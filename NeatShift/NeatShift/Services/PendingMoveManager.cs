using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NeatShift.Services
{
    internal class PendingMove
    {
        public string DestinationPath { get; set; } = string.Empty;
        public List<string> SourcePaths { get; set; } = new();
    }

    internal static class PendingMoveManager
    {
        private static readonly string FilePath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "NeatShift", "pending_move.json");

        public static void Save(PendingMove move)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(move));
        }

        public static PendingMove? LoadAndDelete()
        {
            if (!File.Exists(FilePath)) return null;
            var json = File.ReadAllText(FilePath);
            File.Delete(FilePath);
            return JsonSerializer.Deserialize<PendingMove>(json);
        }
    }
} 
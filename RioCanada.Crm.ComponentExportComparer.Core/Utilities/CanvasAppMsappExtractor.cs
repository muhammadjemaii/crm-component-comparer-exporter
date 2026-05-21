using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RioCanada.Crm.ComponentExportComparer.Core.Utilities
{
    /// <summary>
    /// Extracts the contents of a .msapp file (a ZIP archive) into a flat list of
    /// named entries so that each file can be individually compared between environments.
    /// </summary>
    public static class CanvasAppMsappExtractor
    {
        /// <summary>
        /// Represents a single file extracted from a .msapp ZIP.
        /// </summary>
        public class MsappEntry
        {
            /// <summary>Relative path inside the .msapp, e.g. "Controls/Screen1.json".</summary>
            public string EntryPath { get; set; }

            /// <summary>
            /// UTF-8 text content when the entry is a text-based format (.json, .xml, .yaml, .fx, .pa.yaml).
            /// Null for binary entries.
            /// </summary>
            public string TextContent { get; set; }

            /// <summary>Raw bytes for binary entries (images, etc.). Null for text entries.</summary>
            public byte[] BinaryContent { get; set; }

            /// <summary>True when the entry is stored as plain text.</summary>
            public bool IsText { get; set; }

            /// <summary>Original compressed size in bytes.</summary>
            public long CompressedSize { get; set; }
        }

        // File extensions treated as plain text inside a .msapp
        private static readonly HashSet<string> TextExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".json", ".xml", ".yaml", ".yml", ".fx", ".pa.yaml", ".txt", ".html", ".htm", ".js", ".css"
        };

        /// <summary>
        /// Extracts all entries from the .msapp binary.
        /// </summary>
        /// <param name="msappBytes">Raw bytes of the .msapp file.</param>
        /// <returns>List of extracted entries, sorted by path.</returns>
        public static List<MsappEntry> Extract(byte[] msappBytes)
        {
            var result = new List<MsappEntry>();

            if (msappBytes == null || msappBytes.Length == 0)
                return result;

            using (var stream = new MemoryStream(msappBytes))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip directory entries (ZipArchive entries that end with '/')
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    var entryPath = entry.FullName.Replace('\\', '/');
                    var ext = GetExtension(entryPath);
                    var isText = TextExtensions.Contains(ext);

                    var msappEntry = new MsappEntry
                    {
                        EntryPath = entryPath,
                        IsText = isText,
                        CompressedSize = entry.CompressedLength,
                    };

                    using (var entryStream = entry.Open())
                    using (var ms = new MemoryStream())
                    {
                        entryStream.CopyTo(ms);
                        var bytes = ms.ToArray();

                        if (isText)
                        {
                            // Detect and strip UTF-8 BOM if present
                            msappEntry.TextContent = DecodeText(bytes);
                        }
                        else
                        {
                            msappEntry.BinaryContent = bytes;
                        }
                    }

                    result.Add(msappEntry);
                }
            }

            result.Sort((a, b) => string.Compare(a.EntryPath, b.EntryPath, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        /// <summary>
        /// Returns the "deepest" extension. For "Controls/Screen1.pa.yaml" this returns ".pa.yaml";
        /// for plain "metadata.json" it returns ".json".
        /// </summary>
        private static string GetExtension(string path)
        {
            var name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(name)) return string.Empty;

            // Handle double extensions like .pa.yaml
            var firstDot = name.IndexOf('.');
            if (firstDot >= 0)
                return name.Substring(firstDot).ToLower();

            return string.Empty;
        }

        private static string DecodeText(byte[] bytes)
        {
            // Strip UTF-8 BOM
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}

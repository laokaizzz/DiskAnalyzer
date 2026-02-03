using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DiskAnalyzer.Services
{
    public static class FileClassifier
    {
        // Level 0: Unsafe (System)
        private static readonly List<string> SystemPaths = new List<string>
        {
            @"C:\Windows",
            @"C:\Program Files",
            @"C:\Program Files (x86)",
            @"C:\Drivers",
            @"C:\Intel",
            @"C:\AMD"
        };

        // Level 1: Safe (User)
        private static readonly List<string> UserPaths = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Videos"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Pictures")
        };

        private static readonly HashSet<string> SafeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".rar", ".iso", ".vmdk", ".mp4", ".mkv", ".msi", ".log"
        };

        public static int Classify(string fullPath, string extension)
        {
            // Check Level 0 (Unsafe) - Path Prefix
            foreach (var sysPath in SystemPaths)
            {
                if (fullPath.StartsWith(sysPath, StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }
            }

            // Check Level 1 (Safe) - User Path
            foreach (var userPath in UserPaths)
            {
                if (fullPath.StartsWith(userPath, StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }
            }

            // Check Level 1 (Safe) - Extension
            if (SafeExtensions.Contains(extension))
            {
                return 1;
            }

            // Level 2: Unknown
            return 2;
        }
    }
}

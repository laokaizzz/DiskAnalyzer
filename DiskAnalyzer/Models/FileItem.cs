using System;

namespace DiskAnalyzer.Models
{
    public class FileItem
    {
        public string FullPath { get; set; }
        public long SizeBytes { get; set; }
        public string Ext { get; set; }
        public DateTime LastWrite { get; set; }
        public int SafetyLevel { get; set; } // 0: Unsafe, 1: Safe, 2: Unknown

        public string SizeFormatted
        {
            get
            {
                double size = SizeBytes;
                string[] units = { "B", "KB", "MB", "GB", "TB" };
                int unitIndex = 0;
                while (size >= 1024 && unitIndex < units.Length - 1)
                {
                    size /= 1024;
                    unitIndex++;
                }
                return $"{size:0.00} {units[unitIndex]}";
            }
        }
        
        public string SafetyLevelText
        {
            get
            {
                switch (SafetyLevel)
                {
                    case 0: return "不可删除";
                    case 1: return "可删除";
                    case 2: return "未知";
                    default: return "未知";
                }
            }
        }
    }
}

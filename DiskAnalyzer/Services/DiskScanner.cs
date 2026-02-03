using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiskAnalyzer.Models;

namespace DiskAnalyzer.Services
{
    public class DiskScanner
    {
        private volatile bool _isPaused = false;
        
        // Events for UI updates
        public event Action<string> CurrentPathChanged;

        public async Task<List<FileItem>> ScanAsync(IEnumerable<string> paths, long sizeThresholdBytes, CancellationToken token)
        {
            var results = new ConcurrentBag<FileItem>();
            int processedCount = 0;
            
            await Task.Run(() =>
            {
                foreach (var rootPath in paths)
                {
                    if (token.IsCancellationRequested) break;
                    if (!Directory.Exists(rootPath)) continue;

                    var options = new EnumerationOptions
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                        AttributesToSkip = FileAttributes.ReparsePoint
                    };

                    try
                    {
                        // Using EnumerateFiles for stream processing
                        var files = Directory.EnumerateFiles(rootPath, "*", options);

                        foreach (var file in files)
                        {
                            if (token.IsCancellationRequested) break;
                            
                            // Pause logic
                            while (_isPaused)
                            {
                                if (token.IsCancellationRequested) break;
                                Thread.Sleep(100);
                            }

                            // Throttle UI updates to avoid freezing
                            processedCount++;
                            if (processedCount % 50 == 0)
                            {
                                CurrentPathChanged?.Invoke(file);
                            }

                            try
                            {
                                var info = new FileInfo(file);
                                if (info.Length >= sizeThresholdBytes)
                                {
                                    var item = new FileItem
                                    {
                                        FullPath = file,
                                        SizeBytes = info.Length,
                                        Ext = info.Extension,
                                        LastWrite = info.LastWriteTime,
                                        SafetyLevel = FileClassifier.Classify(file, info.Extension)
                                    };
                                    results.Add(item);
                                }
                            }
                            catch (Exception ex)
                            {
                                NLog.LogManager.GetCurrentClassLogger().Error(ex, $"Access denied or error: {file}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                         NLog.LogManager.GetCurrentClassLogger().Error(ex, $"Error enumerating directory: {rootPath}");
                    }
                }
            }, token);

            return results.OrderByDescending(x => x.SizeBytes).ToList();
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }
    }
}

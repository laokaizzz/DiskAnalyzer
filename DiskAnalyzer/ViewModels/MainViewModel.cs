using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskAnalyzer.Models;
using DiskAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CsvHelper;
using System.Globalization;
using System.IO;

namespace DiskAnalyzer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly DiskScanner _scanner;
        private CancellationTokenSource _cts;
        private List<FileItem> _allResults;
        
        private ObservableCollection<FileItem> _filteredResults;
        private bool _isScanning;
        private bool _isPaused;
        private string _currentPath;
        private double _sizeThresholdMB = 100;
        private bool _showUnsafe = true;
        private bool _showSafe = true;
        private bool _showUnknown = true;
        private string _extensionFilterText;

        public MainViewModel()
        {
            _scanner = new DiskScanner();
            _scanner.CurrentPathChanged += (path) => 
            {
                Application.Current.Dispatcher.Invoke(() => CurrentPath = path);
            };
            FilteredResults = new ObservableCollection<FileItem>();
            _allResults = new List<FileItem>();
            
            StartScanCommand = new AsyncRelayCommand(StartScan);
            PauseScanCommand = new RelayCommand(PauseScan);
            ResumeScanCommand = new RelayCommand(ResumeScan);
            CancelScanCommand = new RelayCommand(CancelScan);
            DeleteCommand = new RelayCommand<object>(DeleteSelected);
            ExportCommand = new RelayCommand(ExportData);
        }

        public ObservableCollection<FileItem> FilteredResults
        {
            get => _filteredResults;
            set => SetProperty(ref _filteredResults, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            set 
            {
                SetProperty(ref _isScanning, value);
                (StartScanCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                (PauseScanCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (ResumeScanCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (CancelScanCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                SetProperty(ref _isPaused, value);
                (PauseScanCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (ResumeScanCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        public double SizeThresholdMB
        {
            get => _sizeThresholdMB;
            set
            {
                if (SetProperty(ref _sizeThresholdMB, value))
                    ApplyFilters();
            }
        }

        public bool ShowUnsafe
        {
            get => _showUnsafe;
            set
            {
                if (SetProperty(ref _showUnsafe, value))
                    ApplyFilters();
            }
        }

        public bool ShowSafe
        {
            get => _showSafe;
            set
            {
                if (SetProperty(ref _showSafe, value))
                    ApplyFilters();
            }
        }

        public bool ShowUnknown
        {
            get => _showUnknown;
            set
            {
                if (SetProperty(ref _showUnknown, value))
                    ApplyFilters();
            }
        }

        public string ExtensionFilterText
        {
            get => _extensionFilterText;
            set
            {
                if (SetProperty(ref _extensionFilterText, value))
                    ApplyFilters();
            }
        }

        public IAsyncRelayCommand StartScanCommand { get; }
        public IRelayCommand PauseScanCommand { get; }
        public IRelayCommand ResumeScanCommand { get; }
        public IRelayCommand CancelScanCommand { get; }
        public IRelayCommand<object> DeleteCommand { get; }
        public IRelayCommand ExportCommand { get; }

        private async Task StartScan()
        {
            if (IsScanning) return;

            IsScanning = true;
            IsPaused = false;
            _cts = new CancellationTokenSource();
            _allResults.Clear();
            FilteredResults.Clear();

            try
            {
                var paths = new[] { "C:\\" }; 
                long thresholdBytes = (long)(SizeThresholdMB * 1024 * 1024);
                
                var results = await _scanner.ScanAsync(paths, thresholdBytes, _cts.Token);
                
                _allResults = results;
                ApplyFilters();
            }
            catch (OperationCanceledException)
            {
                CurrentPath = "扫描已取消";
            }
            finally
            {
                IsScanning = false;
                IsPaused = false;
                CurrentPath = "扫描完成";
            }
        }

        private void PauseScan()
        {
            if (IsScanning && !IsPaused)
            {
                _scanner.Pause();
                IsPaused = true;
            }
        }

        private void ResumeScan()
        {
            if (IsScanning && IsPaused)
            {
                _scanner.Resume();
                IsPaused = false;
            }
        }

        private void CancelScan()
        {
            _cts?.Cancel();
        }

        private void ApplyFilters()
        {
            if (_allResults == null) return;

            var query = _allResults.AsEnumerable();

            query = query.Where(x => 
                (x.SafetyLevel == 0 && ShowUnsafe) ||
                (x.SafetyLevel == 1 && ShowSafe) ||
                (x.SafetyLevel == 2 && ShowUnknown)
            );

            if (!string.IsNullOrWhiteSpace(ExtensionFilterText))
            {
                var ext = ExtensionFilterText.Trim().ToLower();
                if (!ext.StartsWith(".")) ext = "." + ext;
                query = query.Where(x => x.Ext.ToLower().Contains(ext));
            }
            
            long thresholdBytes = (long)(SizeThresholdMB * 1024 * 1024);
            query = query.Where(x => x.SizeBytes >= thresholdBytes);

            FilteredResults = new ObservableCollection<FileItem>(query);
        }

        private void DeleteSelected(object selectedItems)
        {
             var list = selectedItems as System.Collections.IList;
             if (list == null || list.Count == 0) return;

             var items = list.Cast<FileItem>().ToList();
             
             var result = MessageBox.Show($"Are you sure you want to delete {items.Count} files?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
             if (result == MessageBoxResult.Yes)
             {
                 foreach (var item in items)
                 {
                     try
                     {
                         if (File.Exists(item.FullPath))
                         {
                             File.Delete(item.FullPath);
                             _allResults.Remove(item);
                         }
                     }
                     catch (Exception ex)
                     {
                         NLog.LogManager.GetCurrentClassLogger().Error(ex, "Delete failed");
                     }
                 }
                 ApplyFilters();
             }
        }

        private void ExportData()
        {
            try
            {
                using (var writer = new StreamWriter("export.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(FilteredResults);
                }
                MessageBox.Show("已导出至 export.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败: " + ex.Message);
            }
        }
    }
}

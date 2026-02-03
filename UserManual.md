# Disk Analyzer - User Manual

## Overview
Disk Analyzer is a tool to scan your C: drive (or other paths) for large files (>100MB) and classify them as Safe to delete, System files (Unsafe), or Unknown.

## Features
- **Fast Scanning**: Multi-threaded scanning engine.
- **Smart Classification**:
  - **Red**: Unsafe (System files, e.g., Windows, Program Files).
  - **Green**: Safe (User documents, downloads, large archives/videos).
  - **Orange**: Unknown (Review carefully).
- **Filtering**: Filter by size, safety level, or extension.
- **Actions**: Export to CSV or Delete selected files.

## Usage
1. **Start Scan**: Click "Start Scan". The tool defaults to scanning C:\.
2. **Pause/Resume**: Use the buttons to control the scan.
3. **Filtering**:
   - Adjust the **Size Threshold** slider to filter files by size.
   - Toggle **Safety Level** checkboxes.
   - Type an extension (e.g., `.mp4`) in the text box.
4. **Delete**: Select files in the list (hold Ctrl/Shift for multiple) and click "Delete Selected".
   - **Warning**: Deleted files are permanently removed.
5. **Export**: Click "Export CSV" to save the current list to `export.csv`.

## Troubleshooting
- **Logs**: Check `%AppData%\DiskAnalyzer\logs` for error logs.
- **Permissions**: Run as Administrator to scan more system folders.

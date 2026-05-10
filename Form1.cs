using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace mgxparser
{
    public partial class Form1 : Form
    {
        private string _currentFolder;
        private int _sortColumn = 2; // 默认按日期列排序
        private bool _sortAscending;
        private CancellationTokenSource _cts;
        private string _currentGuid;
        private FileInfo _currentSearchedFile;
        private static readonly string LastFolderFile = Path.Combine(
            Path.GetTempPath(), "mgxparser_lastfolder.txt");

        public Form1()
        {
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
            InitializeComponent();
            lvFiles.ListViewItemSorter = new ListViewItemComparer();
            LoadLastFolder();
        }

        private void SetStatus(string text, int progress = -1)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, int>(SetStatus), text, progress);
                return;
            }
            statusLabel.Text = text;
            if (progress >= 0)
            {
                statusProgress.Visible = true;
                statusProgress.Value = Math.Min(progress, 100);
            }
            else
            {
                statusProgress.Visible = false;
                statusProgress.Value = 0;
            }
        }

        private void LoadLastFolder()
        {
            try
            {
                if (File.Exists(LastFolderFile))
                {
                    var path = File.ReadAllText(LastFolderFile).Trim();
                    if (Directory.Exists(path))
                    {
                        _currentFolder = path;
                        txtFolderPath.Text = _currentFolder;
                        RefreshFileList();
                    }
                }
            }
            catch
            {
                // ignore errors reading last folder
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _currentFolder = dialog.SelectedPath;
                    txtFolderPath.Text = _currentFolder;
                    RefreshFileList();
                    ClearPlayerInfo();
                    SaveLastFolder();
                }
            }
        }

        private void RefreshFileList()
        {
            lvFiles.BeginUpdate();
            lvFiles.Items.Clear();

            try
            {
                var files = Directory.GetFiles(_currentFolder, "*.mgx")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                foreach (var fi in files)
                {
                    var item = new ListViewItem(fi.Name);
                    item.SubItems.Add(FormatSize(fi.Length));
                    item.SubItems.Add(fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = fi;
                    lvFiles.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取文件夹失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lvFiles.EndUpdate();
        }

        private void SaveLastFolder()
        {
            try
            {
                File.WriteAllText(LastFolderFile, _currentFolder);
            }
            catch
            {
                // ignore write errors
            }
        }

        private static string FormatResignTime(int? ms)
        {
            if (ms == null || ms.Value <= 0)
                return "";
            return TimeSpan.FromMilliseconds(ms.Value).ToString(@"hh\:mm\:ss");
        }

        private static string FormatDuration(long ms)
        {
            if (ms <= 0)
                return "";
            var ts = TimeSpan.FromMilliseconds(ms);
            if (ts.Hours > 0)
                return ts.ToString(@"h\:mm\:ss");
            return ts.ToString(@"mm\:ss");
        }

        private static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {suffixes[order]}";
        }

        private async void lvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count == 0)
            {
                ClearPlayerInfo();
                btnDelete.Enabled = false;
                lvPlayers.Enabled = false;
                tsmiUpload.Text = "上传录像";
                btnUploadAll.Text = "全部上传";
                return;
            }

            if (lvFiles.SelectedItems.Count > 1)
            {
                ClearPlayerInfo();
                lvPlayers.Enabled = false;
                btnDelete.Enabled = true;
                tsmiUpload.Text = $"上传 {lvFiles.SelectedItems.Count} 个";
                btnUploadAll.Text = $"上传 {lvFiles.SelectedItems.Count} 个";
                return;
            }

            lvPlayers.Enabled = true;
            btnUploadAll.Text = "全部上传";
            tsmiUpload.Text = "上传录像";

            var fi = lvFiles.SelectedItems[0].Tag as FileInfo;
            if (fi == null)
                return;

            btnDelete.Enabled = true;
            await LoadGameInfo(fi.FullName);
        }

        private async Task LoadGameInfo(string filePath)
        {
            lblMatchup.Text = "对阵: 加载中...";
            lvPlayers.Items.Clear();

            try
            {
                var info = await Task.Run(() => MgxParser.ParseGameInfo(filePath));

                _currentGuid = info.guid;
                _currentSearchedFile = new FileInfo(filePath);
                lnkSearch.Visible = true;
                ResetSearchLink();

                var matchup = info.matchup;
                var durStr = FormatDuration(info.duration);
                if (matchup != null && matchup.Count == 2)
                    lblMatchup.Text = $"对阵: {matchup[0]}v{matchup[1]}  时长: {durStr}";
                else
                    lblMatchup.Text = $"对阵: -  时长: {durStr}";

                lvPlayers.BeginUpdate();
                var players = info.players
                    .Where(p => p.playertype != 0)
                    .OrderBy(p => p.slot)
                    .ToList();

                for (int i = 0; i < 8; i++)
                {
                    if (i < players.Count)
                    {
                        var p = players[i];
                        var item = new ListViewItem(p.name ?? "");
                        item.SubItems.Add(p.civ ?? "");
                        item.SubItems.Add(p.teamid?.ToString() ?? "-");
                        item.SubItems.Add(p.winner == true ? "✓" : "");
                        item.SubItems.Add(FormatResignTime(p.resigned));
                        lvPlayers.Items.Add(item);
                    }
                    else
                    {
                        var item = new ListViewItem("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        lvPlayers.Items.Add(item);
                    }
                }
                lvPlayers.EndUpdate();
            }
            catch (Exception ex)
            {
                lblMatchup.Text = "对阵: 解析失败";
                _currentGuid = null;
                _currentSearchedFile = null;
                ResetSearchLink();
                MessageBox.Show($"解析录像文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearPlayerInfo()
        {
            lblMatchup.Text = "对阵: -";
            lvPlayers.Items.Clear();
            _currentGuid = null;
            _currentSearchedFile = null;
            lnkSearch.Visible = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedFile();
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定要清理配置缓存吗？这将清除保存的文件夹路径并重置界面。",
                "清理缓存", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                if (File.Exists(LastFolderFile))
                    File.Delete(LastFolderFile);
            }
            catch (Exception ex)
            {
                WriteLog("error", $"清理配置文件失败", ex.Message);
            }

            // Reset UI to defaults
            _currentFolder = null;
            txtFolderPath.Text = "";
            lvFiles.Items.Clear();
            ClearPlayerInfo();
            btnDelete.Enabled = false;
            lvPlayers.Enabled = false;
            btnUploadAll.Text = "全部上传";
            tsmiUpload.Text = "上传录像";

            SetStatus("配置缓存已清理");
        }

        private void ctxDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedFile();
        }

        private void ctxCopyPath_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count == 0)
                return;

            var fi = lvFiles.SelectedItems[0].Tag as FileInfo;
            if (fi != null)
            {
                Clipboard.SetText(fi.FullName);
            }
        }

        private void ctxMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var selCount = lvFiles.SelectedItems.Count;
            tsmiUpload.Text = selCount > 1 ? $"上传 {selCount} 个" : "上传录像";
        }

        private async void ctxUpload_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count == 0)
                return;

            var files = lvFiles.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as FileInfo)
                .Where(fi => fi != null)
                .ToList();

            if (files.Count == 0)
                return;

            if (files.Count == 1)
            {
                var fi = files[0];
                SetStatus($"正在上传: {fi.Name}", 0);
                var ok = await UploadFileAsync(fi);
                SetStatus(ok == true ? $"\"{fi.Name}\" 上传成功" : $"\"{fi.Name}\" 上传失败");
            }
            else
            {
                int success = 0, fail = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    var fi = files[i];
                    int pct = (i * 100) / files.Count;
                    SetStatus($"正在上传: {fi.Name} ({i + 1}/{files.Count})", pct);
                    var ok = await UploadFileAsync(fi);
                    if (ok == true)
                        success++;
                    else
                        fail++;
                }
                SetStatus($"上传完成。成功: {success}, 失败: {fail}", 100);
            }
        }

        private async void btnUploadAll_Click(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                return;
            }

            List<FileInfo> files;
            bool uploadingSelected = lvFiles.SelectedItems.Count > 1;

            if (uploadingSelected)
            {
                files = lvFiles.SelectedItems
                    .Cast<ListViewItem>()
                    .Select(item => item.Tag as FileInfo)
                    .Where(fi => fi != null)
                    .ToList();
            }
            else
            {
                if (lvFiles.Items.Count == 0 || string.IsNullOrEmpty(_currentFolder))
                    return;
                files = Directory.GetFiles(_currentFolder, "*.mgx")
                    .Select(f => new FileInfo(f))
                    .OrderBy(f => f.Name)
                    .ToList();
            }

            if (files.Count == 0)
                return;

            btnUploadAll.Text = "取消上传";
            int success = 0, fail = 0, skipped = 0;

            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            SetStatus($"准备上传 {files.Count} 个文件...", 0);

            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    if (ct.IsCancellationRequested)
                    {
                        SetStatus($"已取消。已上传: {i}/{files.Count}, 成功: {success}, 失败: {fail}");
                        return;
                    }

                    var fi = files[i];
                    int pct = (i * 100) / files.Count;
                    SetStatus($"已上传: {i}/{files.Count}, 成功: {success}, 失败: {fail} — 正在上传: {fi.Name}", pct);

                    var result = await UploadFileAsync(fi, ct);
                    if (result == true)
                        success++;
                    else if (result == false)
                        fail++;
                    else
                        skipped++;
                }

                SetStatus($"上传完成。成功: {success}, 失败: {fail}, 跳过: {skipped}", 100);
            }
            catch (OperationCanceledException)
            {
                SetStatus("上传已取消");
            }
            catch (Exception ex)
            {
                SetStatus($"上传出错: {ex.Message}");
            }
            finally
            {
                _cts = null;
                var selCount = lvFiles.SelectedItems.Count;
                btnUploadAll.Text = selCount > 1 ? $"上传 {selCount} 个" : "全部上传";

                if (lvFiles.SelectedItems.Count == 1)
                {
                    var fi = lvFiles.SelectedItems[0].Tag as FileInfo;
                    if (fi != null)
                        _ = LoadGameInfo(fi.FullName);
                }
                else
                {
                    lblMatchup.Text = "对阵: -";
                }
            }
        }

        private static readonly HttpClient UploadClient = new HttpClient();
        private const string UploadUrl = "https://manage.aocrec.com/upload";
        private const long MaxUploadSize = 30 * 1024 * 1024;

        private async Task<bool?> UploadFileAsync(FileInfo fi, CancellationToken ct = default)
        {
            if (fi.Length > MaxUploadSize)
                return null; // skipped

            string responseBody = null;

            try
            {
                using (var form = new MultipartFormDataContent())
                using (var fileStream = File.OpenRead(fi.FullName))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.TryAddWithoutValidation(
                        "Content-Disposition",
                        $"form-data; name=\"recfile\"; filename*=UTF-8''{Uri.EscapeDataString(fi.Name)}");
                    form.Add(fileContent);
                    form.Add(new StringContent(
                        new DateTimeOffset(fi.LastWriteTime).ToUnixTimeMilliseconds().ToString()),
                        "lastmod");

                    var response = await UploadClient.PostAsync(UploadUrl, form, ct);
                    responseBody = await response.Content.ReadAsStringAsync();

                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    Dictionary<string, object> dict;

                    try
                    {
                        dict = serializer.Deserialize<Dictionary<string, object>>(responseBody);
                    }
                    catch
                    {
                        WriteLog("error", $"{fi.Name}: 响应非 JSON", responseBody);
                        return false;
                    }

                    if (dict != null && dict.ContainsKey("guid"))
                        return true;

                    var errMsg = dict != null && dict.ContainsKey("error") ? dict["error"].ToString() : responseBody;
                    WriteLog("upload", $"{fi.Name}: {errMsg}", responseBody);
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                WriteLog("error", $"{fi.Name}: 异常: {ex.Message}", ex.ToString());
                return false;
            }
        }

        private static readonly string LogDir = AppDomain.CurrentDomain.BaseDirectory;

        private static void WriteLog(string type, string message, string detail = null)
        {
#if !DEBUG
            if (type != "error" && string.IsNullOrEmpty(detail))
                return;
#endif
            try
            {
                var logPath = Path.Combine(LogDir, "mgxparser.log");
                using (var w = File.AppendText(logPath))
                {
                    w.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}");
                    if (detail != null)
                        w.WriteLine(detail);
                    w.WriteLine(new string('-', 60));
                }
            }
            catch { }
        }

        private void ResetSearchLink()
        {
            lnkSearch.Text = "上传到档案库";
            lnkSearch.Tag = null;
            lnkSearch.LinkVisited = false;
        }

        private async void lnkSearch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var tag = lnkSearch.Tag as string;

            if (tag != null && tag.StartsWith("http"))
            {
                System.Diagnostics.Process.Start(tag);
                return;
            }

            // Search
            if (string.IsNullOrEmpty(_currentGuid))
            {
                lnkSearch.Text = "没有 GUID";
                SetStatus("没有可查询的 GUID");
                return;
            }

            lnkSearch.Enabled = false;
            lnkSearch.Text = "查询中...";
            SetStatus("正在查询档案库...", 50);

            try
            {
                var payload = $"{{\"size\":8,\"query\":{{\"term\":{{\"guid\":\"{_currentGuid}\"}}}},\"sort\":[{{\"duration\":\"desc\"}}]}}";
                WriteLog("search", $"发送查询: {_currentGuid}");

                var json = await Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        var authBytes = System.Text.Encoding.ASCII.GetBytes("aocrec:aocrec");
                        client.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(authBytes);
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        var bytes = client.UploadData(
                            "https://es1.aocrec.com/mgxhub1/_search",
                            System.Text.Encoding.UTF8.GetBytes(payload));
                        return System.Text.Encoding.UTF8.GetString(bytes);
                    }
                });

                WriteLog("search", "查询响应", json?.Substring(0, Math.Min(json.Length, 500)));

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var result = serializer.Deserialize<Dictionary<string, object>>(json);

                bool found = false;

                if (result != null && result.ContainsKey("hits"))
                {
                    var hits = result["hits"] as Dictionary<string, object>;
                    if (hits != null && hits.ContainsKey("hits"))
                    {
                        var documents = hits["hits"] as System.Collections.ArrayList;
                        if (documents != null && documents.Count > 0)
                            found = true;
                    }
                }

                if (found)
                {
                    var detailUrl = $"https://aocrec.com/#{_currentGuid}";
                    lnkSearch.Text = "查看详情";
                    lnkSearch.Tag = detailUrl;
                    SetStatus($"录像已被帝国时代档案库收录");
                }
                else
                {
                    // Auto-upload when not found, no second click needed
                    lnkSearch.Text = "正在上传...";
                    SetStatus($"正在上传: {_currentSearchedFile?.Name ?? "未知"}", 0);

                    if (_currentSearchedFile != null)
                    {
                        var ok = await UploadFileAsync(_currentSearchedFile);
                        if (ok == true)
                        {
                            lnkSearch.Text = "查询档案库";
                            lnkSearch.Tag = null;
                            SetStatus($"\"{_currentSearchedFile.Name}\" 上传成功");
                        }
                        else
                        {
                            lnkSearch.Text = "上传到档案库";
                            lnkSearch.Tag = null;
                            SetStatus($"\"{_currentSearchedFile.Name}\" 上传失败");
                        }
                    }
                    else
                    {
                        lnkSearch.Text = "上传到档案库";
                        lnkSearch.Tag = null;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("error", "查询异常", ex.ToString());
                lnkSearch.Text = "查询失败";
                lnkSearch.Tag = null;
                SetStatus($"查询失败: {ex.Message}");
            }
            finally
            {
                lnkSearch.Enabled = true;
            }
        }

        private void lvFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedFile();
                e.Handled = true;
            }
        }

        private void lvFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            lvFiles.ListViewItemSorter = new ListViewItemComparer
            {
                Column = _sortColumn,
                Ascending = _sortAscending
            };
            lvFiles.Sort();
        }

        private void DeleteSelectedFile()
        {
            if (lvFiles.SelectedItems.Count == 0)
                return;

            var filesToDelete = lvFiles.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as FileInfo)
                .Where(fi => fi != null)
                .ToList();

            if (filesToDelete.Count == 0)
                return;

            int firstIndex = lvFiles.SelectedIndices[0];

            if (chkConfirmDelete.Checked)
            {
                string msg = filesToDelete.Count == 1
                    ? $"确定要删除 \"{filesToDelete[0].Name}\" 吗？此操作会将文件移入回收站。"
                    : $"确定要删除选中的 {filesToDelete.Count} 个文件吗？此操作会将文件移入回收站。";

                var result = MessageBox.Show(msg, "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            int failCount = 0;
            foreach (var fi in filesToDelete)
            {
                try
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                        fi.FullName,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    failCount++;
                    WriteLog("error", $"删除失败: {fi.Name}", ex.Message);
                }
            }

            RefreshFileList();
            ClearPlayerInfo();
            btnDelete.Enabled = false;

            if (failCount > 0)
            {
                MessageBox.Show($"删除了 {filesToDelete.Count - failCount} 个文件，{failCount} 个失败。",
                    "删除结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (lvFiles.Items.Count > 0)
            {
                int newIndex = Math.Min(firstIndex, lvFiles.Items.Count - 1);
                lvFiles.Items[newIndex].Selected = true;
                lvFiles.Items[newIndex].Focused = true;
                lvFiles.Items[newIndex].EnsureVisible();
            }
        }

        private class ListViewItemComparer : IComparer
        {
            public int Column { get; set; }
            public bool Ascending { get; set; } = true;

            public int Compare(object x, object y)
            {
                var itemX = (ListViewItem)x;
                var itemY = (ListViewItem)y;
                int result;

                switch (Column)
                {
                    case 0:
                        result = string.Compare(itemX.Text, itemY.Text,
                            StringComparison.CurrentCultureIgnoreCase);
                        break;
                    case 1:
                        var sizeX = ((FileInfo)itemX.Tag).Length;
                        var sizeY = ((FileInfo)itemY.Tag).Length;
                        result = sizeX.CompareTo(sizeY);
                        break;
                    case 2:
                        var dateX = ((FileInfo)itemX.Tag).LastWriteTime;
                        var dateY = ((FileInfo)itemY.Tag).LastWriteTime;
                        result = dateX.CompareTo(dateY);
                        break;
                    default:
                        result = 0;
                        break;
                }

                return Ascending ? result : -result;
            }
        }
    }
}

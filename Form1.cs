using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private int _sortColumn;
        private bool _sortAscending = true;
        private CancellationTokenSource _cts;
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
                return;
            }

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
                MessageBox.Show($"解析录像文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearPlayerInfo()
        {
            lblMatchup.Text = "对阵: -";
            lvPlayers.Items.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedFile();
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

        private async void ctxUpload_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count == 0)
                return;

            var fi = lvFiles.SelectedItems[0].Tag as FileInfo;
            if (fi == null)
                return;

            SetStatus($"正在上传: {fi.Name}", 0);
            var ok = await UploadFileAsync(fi);
            SetStatus(ok == true ? $"\"{fi.Name}\" 上传成功" : $"\"{fi.Name}\" 上传失败");
        }

        private async void btnUploadAll_Click(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                return;
            }

            if (lvFiles.Items.Count == 0 || string.IsNullOrEmpty(_currentFolder))
                return;

            btnUploadAll.Text = "取消上传";
            int success = 0, fail = 0, skipped = 0;
            var files = Directory.GetFiles(_currentFolder, "*.mgx")
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.Name)
                .ToList();

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
                btnUploadAll.Text = "全部上传";
                if (lvFiles.SelectedItems.Count > 0)
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
                        $"form-data; name=\"recfile\"; filename=\"{fi.Name}\"");
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
                        // Response is not valid JSON — treat raw text as error
#if DEBUG
                        WriteUploadDebugLog(fi.Name, responseBody, null);
#endif
                        return false;
                    }

                    if (dict != null && dict.ContainsKey("guid"))
                        return true;

                    var errMsg = dict != null && dict.ContainsKey("error") ? dict["error"].ToString() : responseBody;
#if DEBUG
                    WriteUploadDebugLog(fi.Name, responseBody, errMsg);
#endif
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                WriteUploadDebugLog(fi.Name, responseBody, ex.ToString());
#endif
                return false;
            }
        }

#if DEBUG
        private static void WriteUploadDebugLog(string fileName, string responseBody, string errorInfo)
        {
            try
            {
                var logPath = Path.Combine(Environment.CurrentDirectory, "upload_debug.log");
                using (var w = File.AppendText(logPath))
                {
                    w.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 文件: {fileName}");
                    w.WriteLine($"错误: {errorInfo}");
                    if (responseBody != null)
                        w.WriteLine($"响应: {responseBody}");
                    w.WriteLine(new string('-', 60));
                }
            }
            catch { }
        }
#endif

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

            var fi = lvFiles.SelectedItems[0].Tag as FileInfo;
            if (fi == null)
                return;

            int selectedIndex = lvFiles.SelectedIndices[0];

            if (chkConfirmDelete.Checked)
            {
                var result = MessageBox.Show(
                    $"确定要删除 \"{fi.Name}\" 吗？此操作会将文件移入回收站。",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                    fi.FullName,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin);

                RefreshFileList();
                ClearPlayerInfo();
                btnDelete.Enabled = false;

                if (lvFiles.Items.Count > 0)
                {
                    int newIndex = Math.Min(selectedIndex, lvFiles.Items.Count - 1);
                    lvFiles.Items[newIndex].Selected = true;
                    lvFiles.Items[newIndex].Focused = true;
                    lvFiles.Items[newIndex].EnsureVisible();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

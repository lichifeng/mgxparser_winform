using System;
using System.Collections;
using System.IO;
using System.Linq;
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
        private static readonly string LastFolderFile = Path.Combine(
            Path.GetTempPath(), "mgxparser_lastfolder.txt");

        public Form1()
        {
            InitializeComponent();
            lvFiles.ListViewItemSorter = new ListViewItemComparer();
            LoadLastFolder();
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

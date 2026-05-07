# mgxparser WinForms GUI — 实现计划

## 背景

为 `mgx.exe`（帝国时代录像解析器 CLI）开发一个 .NET Framework 4.8 WinForms 图形界面外壳。用户需要选择一个包含 `.mgx` 录像文件的文件夹，浏览文件列表，选中后自动显示对局信息（对阵 matchup、玩家名称、民族、赢家、队伍分组），并支持删除操作。

## JSON 格式关键字段（已验证）

```json
{
  "matchup": [1, 7],
  "players": [
    { "name": "...", "civ": "中国", "teamid": 2, "winner": null, "slot": 0 },
    { "name": "BG4QAF 全球直播", "civ": "中国", "teamid": 2, "winner": null, "slot": 1 },
    ...
  ],
  "haswinner": false,
  "filename": "testgame.mgx",
  "filesize": 1165286,
  "lastmod": 1778136969613
}
```

- `teamid` 为 `null` → 观战者，不显示
- `slot` 0 通常是观战者，slot 1-8 是真实玩家
- `playertype`: 0=观战者, 2=真人玩家, 4=AI

## 实现步骤

### 步骤 1：设计 Form1 界面布局（左信息右文件）

使用 `TableLayoutPanel` 分为左右两列：

```
+---------------------------------------------------------------+
| 左侧（录像信息区）              | 右侧（文件操作区）          |
|---------------------------------------------------------------|
| +-----------------------------+ | +---------------------------+ |
| | 对阵: 1v7                  | | | [选择文件夹]             | |
| |                             | | | C:\...\Recorded Games   | |
| | ┌───────┬──────┬────┬────┐ | | +---------------------------+ |
| | │ 玩家名 │ 民族 │队伍│赢家│ | | +---------------------------+ |
| | ├───────┼──────┼────┼────┤ | | | 文件名     | 大小 | 日期  | |
| | │ 玩家1  │ 中国 │ 2  │    │ | | |------------+------+-------| |
| | │ 玩家2  │ 西班牙│ 3  │    │ | | | game1.mgx  | 1MB  | 5/1  | |
| | │ ...    │      │    │    │ | | | game2.mgx  | 892K | 5/2  | |
| | └───────┴──────┴────┴────┘ | | | ...        |      |      | |
| |                             | | +---------------------------+ |
| | [删除当前文件]             | |                               |
| +-----------------------------+ |                               |
+---------------------------------------------------------------+
```

控件清单：

**左侧 — 录像信息区**：
- `Label lblMatchup` — 显示对阵信息（如"1v7"）
- `ListView lvPlayers` — 玩家表格（4列：玩家名、民族、队伍、赢家），只读，无表头可选
- `Button btnDelete` — 删除当前选中文件

**右侧 — 文件操作区**：
- `Button btnSelectFolder` — 选择文件夹按钮
- `TextBox txtFolderPath` — 显示选中文件夹路径（只读）
- `ListView lvFiles` — 文件列表（3列：文件名、大小、修改日期），FullRowSelect + Details 视图
- `ContextMenuStrip ctxMenu` — 右键菜单（删除）

### 步骤 2：实现文件夹选择和文件列表刷新

`btnSelectFolder_Click`:
1. `FolderBrowserDialog` 让用户选择文件夹
2. 将路径显示到 `txtFolderPath`
3. 调用 `RefreshFileList(path)`

`RefreshFileList(string folderPath)`:
1. 清空 `lvFiles.Items`
2. `Directory.GetFiles(folderPath, "*.mgx")` 获取文件
3. 对每个文件构建 `ListViewItem`（文件名、大小、最后修改时间）
4. `Tag` 保存 `FileInfo`

### 步骤 3：实现 mgx.exe 调用和 JSON 解析

**新建文件**: [MgxParser.cs](MgxParser.cs)

`MgxParser.ParseGameInfo(string filePath)`:
1. 启动 `mgx.exe` 进程，参数 `--json --zh "{filePath}"`
2. 捕获 stdout，反序列化 JSON
3. 返回 `GameInfo` 对象

模型类：`GameInfo`（matchup、haswinner、players[]）、`PlayerInfo`（name、civ、teamid、winner、slot、playertype）

### 步骤 4：实现文件选中时显示录像信息

`lvFiles_SelectedIndexChanged`:
1. 获取选中的文件路径
2. `Task.Run` 异步调用 `MgxParser.ParseGameInfo`（避免 UI 卡死）
3. 完成后更新左侧面板：
   - `lblMatchup.Text` = `"{matchup[0]}v{matchup[1]}"`
   - `lvPlayers.Items` 填充玩家数据（过滤 playertype=0 观战者）
   - 列：玩家名、民族（civ）、队伍（teamid）、赢家标记（winner != null 时显示）
   - 按 slot 顺序排列，保持 8 个位置

### 步骤 5：实现删除功能

三种删除入口 → 统一调用 `DeleteSelectedFile()`:
1. `btnDelete_Click`
2. `ctxMenu` 右键"删除"
3. `lvFiles_KeyDown` 检测 `Keys.Delete`

`DeleteSelectedFile()`:
1. 确认有选中文件
2. `MessageBox.Show` 确认对话框
3. `File.Delete(fullPath)`
4. 刷新文件列表，清空左侧玩家信息

### 步骤 6：编译与验证

**编译命令**:
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "C:\Users\lichi\source\repos\mgxparser\mgxparser.csproj" /t:Build /p:Configuration=Debug
```

**验证**:
1. 点击"选择文件夹"，选中项目目录
2. 文件列表中显示 `testgame.mgx`
3. 选中文件，左侧显示"对阵：1v7"，8 个玩家信息正确
4. 右键菜单/按钮/DEL 键均可删除

## 关键文件清单

| 文件 | 操作 |
|------|------|
| [Form1.cs](Form1.cs) | 重写 — 全部 UI 逻辑 |
| [Form1.Designer.cs](Form1.Designer.cs) | 重写 — 控件声明和初始化 |
| **新文件:** [MgxParser.cs](MgxParser.cs) | 新建 — mgx.exe 调用和 JSON 解析 |
| [mgxparser.csproj](mgxparser.csproj) | 无需修改 |
| [Program.cs](Program.cs) | 无需修改 |

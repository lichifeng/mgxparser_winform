namespace mgxparser
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tblLayout = new System.Windows.Forms.TableLayoutPanel();
            this.gbInfo = new System.Windows.Forms.GroupBox();
            this.lblMatchup = new System.Windows.Forms.Label();
            this.lnkSearch = new System.Windows.Forms.LinkLabel();
            this.lvPlayers = new System.Windows.Forms.ListView();
            this.colPlayerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCiv = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTeam = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWinner = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colResign = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkConfirmDelete = new System.Windows.Forms.CheckBox();
            this.btnUploadAll = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.gbFiles = new System.Windows.Forms.GroupBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.colFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFileDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpload = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.tblLayout.SuspendLayout();
            this.gbInfo.SuspendLayout();
            this.gbFiles.SuspendLayout();
            this.ctxMenu.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblLayout
            // 
            this.tblLayout.ColumnCount = 2;
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayout.Controls.Add(this.gbInfo, 0, 0);
            this.tblLayout.Controls.Add(this.gbFiles, 1, 0);
            this.tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayout.Location = new System.Drawing.Point(0, 0);
            this.tblLayout.Name = "tblLayout";
            this.tblLayout.Padding = new System.Windows.Forms.Padding(6);
            this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 547F));
            this.tblLayout.Size = new System.Drawing.Size(1156, 541);
            this.tblLayout.TabIndex = 0;
            // 
            // gbInfo
            // 
            this.gbInfo.Controls.Add(this.lblMatchup);
            this.gbInfo.Controls.Add(this.lnkSearch);
            this.gbInfo.Controls.Add(this.lvPlayers);
            this.gbInfo.Controls.Add(this.chkConfirmDelete);
            this.gbInfo.Controls.Add(this.btnUploadAll);
            this.gbInfo.Controls.Add(this.btnDelete);
            this.gbInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbInfo.Location = new System.Drawing.Point(9, 9);
            this.gbInfo.Name = "gbInfo";
            this.gbInfo.Size = new System.Drawing.Size(566, 541);
            this.gbInfo.TabIndex = 0;
            this.gbInfo.TabStop = false;
            this.gbInfo.Text = "录像信息";
            // 
            // lblMatchup
            // 
            this.lblMatchup.AutoSize = true;
            this.lblMatchup.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblMatchup.Location = new System.Drawing.Point(12, 22);
            this.lblMatchup.Name = "lblMatchup";
            this.lblMatchup.Size = new System.Drawing.Size(59, 22);
            this.lblMatchup.TabIndex = 0;
            this.lblMatchup.Text = "对阵: -";
            // 
            // lnkSearch
            // 
            this.lnkSearch.ActiveLinkColor = System.Drawing.Color.DodgerBlue;
            this.lnkSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkSearch.AutoSize = true;
            this.lnkSearch.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lnkSearch.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkSearch.LinkColor = System.Drawing.Color.DodgerBlue;
            this.lnkSearch.Location = new System.Drawing.Point(437, 24);
            this.lnkSearch.Name = "lnkSearch";
            this.lnkSearch.Size = new System.Drawing.Size(107, 20);
            this.lnkSearch.TabIndex = 5;
            this.lnkSearch.TabStop = true;
            this.lnkSearch.Text = "查询档案库记录";
            this.lnkSearch.Visible = false;
            this.lnkSearch.VisitedLinkColor = System.Drawing.Color.DodgerBlue;
            this.lnkSearch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSearch_LinkClicked);
            // 
            // lvPlayers
            // 
            this.lvPlayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvPlayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPlayerName,
            this.colCiv,
            this.colTeam,
            this.colWinner,
            this.colResign});
            this.lvPlayers.FullRowSelect = true;
            this.lvPlayers.GridLines = true;
            this.lvPlayers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvPlayers.HideSelection = false;
            this.lvPlayers.Location = new System.Drawing.Point(12, 56);
            this.lvPlayers.Name = "lvPlayers";
            this.lvPlayers.Size = new System.Drawing.Size(542, 429);
            this.lvPlayers.TabIndex = 1;
            this.lvPlayers.UseCompatibleStateImageBehavior = false;
            this.lvPlayers.View = System.Windows.Forms.View.Details;
            // 
            // colPlayerName
            // 
            this.colPlayerName.Text = "玩家名";
            this.colPlayerName.Width = 172;
            // 
            // colCiv
            // 
            this.colCiv.Text = "民族";
            this.colCiv.Width = 80;
            // 
            // colTeam
            // 
            this.colTeam.Text = "队伍";
            this.colTeam.Width = 50;
            // 
            // colWinner
            // 
            this.colWinner.Text = "赢家";
            this.colWinner.Width = 57;
            // 
            // colResign
            // 
            this.colResign.Text = "投降时间";
            this.colResign.Width = 95;
            // 
            // chkConfirmDelete
            // 
            this.chkConfirmDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkConfirmDelete.AutoSize = true;
            this.chkConfirmDelete.Checked = true;
            this.chkConfirmDelete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkConfirmDelete.Location = new System.Drawing.Point(12, 490);
            this.chkConfirmDelete.Name = "chkConfirmDelete";
            this.chkConfirmDelete.Size = new System.Drawing.Size(103, 24);
            this.chkConfirmDelete.TabIndex = 3;
            this.chkConfirmDelete.Text = "删除前确认";
            this.chkConfirmDelete.UseVisualStyleBackColor = true;
            // 
            // btnUploadAll
            // 
            this.btnUploadAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUploadAll.Location = new System.Drawing.Point(340, 491);
            this.btnUploadAll.Name = "btnUploadAll";
            this.btnUploadAll.Size = new System.Drawing.Size(108, 27);
            this.btnUploadAll.TabIndex = 4;
            this.btnUploadAll.Text = "全部上传";
            this.btnUploadAll.UseVisualStyleBackColor = true;
            this.btnUploadAll.Click += new System.EventHandler(this.btnUploadAll_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(454, 491);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 27);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "删除录像";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // gbFiles
            // 
            this.gbFiles.Controls.Add(this.btnSelectFolder);
            this.gbFiles.Controls.Add(this.txtFolderPath);
            this.gbFiles.Controls.Add(this.lvFiles);
            this.gbFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbFiles.Location = new System.Drawing.Point(581, 9);
            this.gbFiles.Name = "gbFiles";
            this.gbFiles.Size = new System.Drawing.Size(566, 541);
            this.gbFiles.TabIndex = 1;
            this.gbFiles.TabStop = false;
            this.gbFiles.Text = "文件管理";
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(9, 22);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(90, 27);
            this.btnSelectFolder.TabIndex = 0;
            this.btnSelectFolder.Text = "选择文件夹";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // txtFolderPath
            // 
            this.txtFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolderPath.Location = new System.Drawing.Point(105, 24);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(451, 26);
            this.txtFolderPath.TabIndex = 1;
            // 
            // lvFiles
            // 
            this.lvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFileName,
            this.colFileSize,
            this.colFileDate});
            this.lvFiles.ContextMenuStrip = this.ctxMenu;
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.GridLines = true;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(9, 56);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(547, 462);
            this.lvFiles.TabIndex = 2;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            this.lvFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvFiles_ColumnClick);
            this.lvFiles.SelectedIndexChanged += new System.EventHandler(this.lvFiles_SelectedIndexChanged);
            this.lvFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvFiles_KeyDown);
            // 
            // colFileName
            // 
            this.colFileName.Text = "文件名";
            this.colFileName.Width = 265;
            // 
            // colFileSize
            // 
            this.colFileSize.Text = "大小";
            this.colFileSize.Width = 80;
            // 
            // colFileDate
            // 
            this.colFileDate.Text = "修改日期";
            this.colFileDate.Width = 180;
            // 
            // ctxMenu
            // 
            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopyPath,
            this.tsmiUpload,
            this.tsmiDelete});
            this.ctxMenu.Name = "ctxMenu";
            this.ctxMenu.Size = new System.Drawing.Size(149, 70);
            // 
            // tsmiCopyPath
            // 
            this.tsmiCopyPath.Name = "tsmiCopyPath";
            this.tsmiCopyPath.Size = new System.Drawing.Size(148, 22);
            this.tsmiCopyPath.Text = "复制文件路径";
            this.tsmiCopyPath.Click += new System.EventHandler(this.ctxCopyPath_Click);
            // 
            // tsmiUpload
            // 
            this.tsmiUpload.Name = "tsmiUpload";
            this.tsmiUpload.Size = new System.Drawing.Size(148, 22);
            this.tsmiUpload.Text = "上传录像";
            this.tsmiUpload.Click += new System.EventHandler(this.ctxUpload_Click);
            // 
            // tsmiDelete
            // 
            this.tsmiDelete.Name = "tsmiDelete";
            this.tsmiDelete.Size = new System.Drawing.Size(148, 22);
            this.tsmiDelete.Text = "删除";
            this.tsmiDelete.Click += new System.EventHandler(this.ctxDelete_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.statusProgress});
            this.statusStrip.Location = new System.Drawing.Point(0, 541);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1156, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // statusProgress
            // 
            this.statusProgress.Name = "statusProgress";
            this.statusProgress.Size = new System.Drawing.Size(100, 16);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1156, 563);
            this.Controls.Add(this.tblLayout);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.MinimumSize = new System.Drawing.Size(720, 460);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "帝国时代录像解析器";
            this.tblLayout.ResumeLayout(false);
            this.gbInfo.ResumeLayout(false);
            this.gbInfo.PerformLayout();
            this.gbFiles.ResumeLayout(false);
            this.gbFiles.PerformLayout();
            this.ctxMenu.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TableLayoutPanel tblLayout;
        private System.Windows.Forms.GroupBox gbInfo;
        private System.Windows.Forms.Label lblMatchup;
        private System.Windows.Forms.ListView lvPlayers;
        private System.Windows.Forms.ColumnHeader colPlayerName;
        private System.Windows.Forms.ColumnHeader colCiv;
        private System.Windows.Forms.ColumnHeader colTeam;
        private System.Windows.Forms.ColumnHeader colWinner;
        private System.Windows.Forms.ColumnHeader colResign;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnUploadAll;
        private System.Windows.Forms.LinkLabel lnkSearch;
        private System.Windows.Forms.CheckBox chkConfirmDelete;
        private System.Windows.Forms.GroupBox gbFiles;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.ListView lvFiles;
        private System.Windows.Forms.ColumnHeader colFileName;
        private System.Windows.Forms.ColumnHeader colFileSize;
        private System.Windows.Forms.ColumnHeader colFileDate;
        private System.Windows.Forms.ContextMenuStrip ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyPath;
        private System.Windows.Forms.ToolStripMenuItem tsmiUpload;
        private System.Windows.Forms.ToolStripMenuItem tsmiDelete;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar statusProgress;
    }
}

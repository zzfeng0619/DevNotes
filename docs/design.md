## DevNotes 设计文档（概要）

### 1. 总体架构

- **客户端技术栈**：WPF + .NET 10（使用 `net10.0-windows`）。
- **分层结构**：
  - `DevNotes.App`：UI 层（WPF 窗口、ViewModel、用户设置）。
  - `DevNotes.Domain`：领域模型与仓储接口（如 `Note`、`INoteRepository`）。
  - `DevNotes.Infrastructure`：基础设施层（SQLite 访问、路径管理）。
  - `DevNotes.Core`：通用基类与工具（如 `ObservableObject`、`RelayCommand`）。
  - `DevNotes.Tests`：单元测试（当前主要用于后续扩展）。
- **数据存储**：
  - 主数据：本地 SQLite 数据库 `data/db/notes.db`。
  - 配置与 UI 设置：`data/config/ui-settings.json`。

### 2. 领域模型与仓储

- **Note（开发笔记）**
  - 字段：
    - `Id`：自增主键。
    - `Title`：标题。
    - `Content`：Markdown 文本内容。
    - `CreatedAt` / `UpdatedAt`：创建时间 / 更新时间。
    - `IsPinned`：是否置顶。
    - `IsArchived`：是否归档。
    - `IsDeleted`：是否逻辑删除（用于回收站）。
- **INoteRepository**
  - `IReadOnlyList<Note> GetAll()`：获取所有未删除笔记，按置顶与更新时间排序。
  - `void Add(Note note)`：新增笔记并回填 `Id`。
  - `void Update(Note note)`：更新笔记全部字段。
  - `void Delete(int noteId)`：逻辑删除笔记（`IsDeleted = 1`）。

### 3. SQLite 持久化（Infrastructure）

- **AppDataPaths**
  - `GetDatabaseFilePath()`：返回 `data/db/notes.db` 路径，不存在时自动创建目录。
  - `GetUiSettingsFilePath()`：返回 `data/config/ui-settings.json` 路径，不存在时自动创建目录。
- **SqliteNoteRepository（实现 INoteRepository）**
  - 构造时通过 `AppDataPaths.GetDatabaseFilePath()` 初始化连接字符串，并调用 `EnsureDatabaseCreated()`。
  - 建表语句：
    - 表名：`Notes`
    - 字段：`Id`, `Title`, `Content`, `CreatedAt`, `UpdatedAt`, `IsPinned`, `IsArchived`, `IsDeleted`。
  - 重要行为：
    - `GetAll()`：`WHERE IsDeleted = 0 ORDER BY IsPinned DESC, UpdatedAt DESC`。
    - `Add()`：插入后使用 `last_insert_rowid()` 获取主键并赋值给 `Note.Id`。
    - `Update()`：全字段更新。
    - `Delete()`：仅更新 `IsDeleted = 1`，实现回收站风格的逻辑删除。

### 4. UI 层与 ViewModel

- **MainViewModel（主窗口 ViewModel）**
  - 依赖：
    - `INoteRepository`：用于加载和持久化 `Note`。
  - 主要属性：
    - `ObservableCollection<Note> Notes`：左侧列表绑定的数据源。
    - `Note? SelectedNote`：当前选中/编辑的笔记。
    - `bool ShowMarkdownHints`：是否显示 Markdown 语法提示面板（可由用户控制）。
  - 命令：
    - `NewNoteCommand`：
      - 新建一条默认标题为“未命名笔记”的记录。
      - 初始化 `CreatedAt`、`UpdatedAt` 为当前时间。
      - 通过仓储 `Add` 写入 SQLite，并插入到 `Notes` 集合顶部。
    - `SaveNoteCommand`：
      - 更新 `SelectedNote.UpdatedAt` 为当前时间。
      - 通过仓储 `Update` 将变更写入 SQLite。
    - `DeleteNoteCommand`：
      - 调用仓储 `Delete(SelectedNote.Id)` 将笔记逻辑删除。
      - 从 `Notes` 集合中移除该笔记，并选中下一条可用记录。

- **ObservableObject / RelayCommand（Core）**
  - `ObservableObject`：
    - 封装 `INotifyPropertyChanged`，通过 `SetProperty` 简化属性变更通知。
  - `RelayCommand`：
    - 实现 `ICommand`，用委托方式绑定 UI 与 ViewModel 中的逻辑。

### 5. 主窗口布局与 Markdown 编辑/预览

- **MainWindow.xaml 布局**
  - 左侧：笔记列表
    - `ListBox ItemsSource="{Binding Notes}" SelectedItem="{Binding SelectedNote}"`。
  - 右侧：编辑 + 预览区域
    - 标题输入框：`SelectedNote.Title`。
    - 创建时间 / 最后更新时间展示。
    - `CheckBox`：`IsChecked="{Binding ShowMarkdownHints}"`，控制是否显示 Markdown 提示面板。
    - 下方采用左右分栏：
      - 左：Markdown 文本编辑 `TextBox`（绑定 `SelectedNote.Content`）。
      - 右：`WebBrowser` 作为预览区域。

- **Markdown 渲染**
  - 使用 `Markdig` 库（通过 NuGet 引入）构建 `MarkdownPipeline`，启用高级扩展：
    - 标题、列表、代码块、行内代码等常用语法。
  - 在 `MainWindow` 中：
    - `ContentTextBox_OnTextChanged`：每次文本变更时调用 `UpdateMarkdownPreview`，实现**键入即预览**。
    - `UpdateMarkdownPreview(string markdown)`：
      - 用 Markdig 将 Markdown 转为 HTML。
      - 包装基础 HTML 框架与简单样式（基础字体、代码块背景等）。
      - 通过 `MarkdownPreviewBrowser.NavigateToString(html)` 展示 HTML。

### 6. Markdown 语法提示与快捷按钮

- **设计目标**
  - 帮助不熟悉 Markdown 的用户快速上手。
  - 由用户决定是否显示提示区域，避免干扰熟练用户。

- **UI 实现**
  - 在编辑区域顶部增加：
    - `CheckBox`：文本为“显示 Markdown 语法提示”，绑定到 `MainViewModel.ShowMarkdownHints`。
  - 在右侧编辑区域的 Markdown/预览分栏 Grid 中：
    - 新增一行，用于放置 Markdown 提示和快捷插入按钮：
      - 绑定可见性：
        - `Visibility="{Binding ShowMarkdownHints, Converter={StaticResource BoolToVisibilityConverter}}"`。
      - 包含的按钮：
        - “# 标题”：插入或包裹为一级标题。
        - “**加粗**”：为选中文本加粗，或插入 `**加粗文本**`。
        - “代码块”：插入围栏代码块模板（默认语言为 `csharp`）。
        - “- 列表项”：插入 Markdown 列表项前缀。

- **插入逻辑（MainWindow.xaml.cs）**
  - 使用 `InsertMarkdownSnippet(before, after, placeholder)` 辅助方法：
    - 若当前有选中文本：
      - 用 `before + 选中文本 + after` 替换选中内容。
    - 若无选中文本：
      - 在光标处插入 `before + placeholder + after`，并选中 placeholder 以便继续编辑。
  - 各按钮点击事件：
    - `HeadingButton_OnClick`：调用 `InsertMarkdownSnippet("# ", "", "标题")`。
    - `BoldButton_OnClick`：调用 `InsertMarkdownSnippet("**", "**", "加粗文本")`。
    - `CodeBlockButton_OnClick`：调用 `InsertMarkdownSnippet("```csharp\n", "\n```", "// 代码内容")`。
    - `ListItemButton_OnClick`：调用 `InsertMarkdownSnippet("- ", "", "列表项")`。

### 7. UI 设置持久化（ShowMarkdownHints）

- **UiSettings / UiSettingsService（App 层）**
  - `UiSettings`：
    - `bool ShowMarkdownHints`：控制是否显示 Markdown 语法提示面板，默认值为 `true`。
  - `UiSettingsService`：
    - `Load()`：
      - 通过 `AppDataPaths.GetUiSettingsFilePath()` 获取配置路径（`data/config/ui-settings.json`）。
      - 如果文件存在，使用 `System.Text.Json` 反序列化为 `UiSettings`。
      - 读取失败或文件缺失时返回默认 `UiSettings`。
    - `Save(UiSettings settings)`：
      - 将 `settings` 序列化为缩进格式 JSON。
      - 写入到 `data/config/ui-settings.json`。

- **MainWindow 中的使用方式**
  - 构造函数中：
    - 先调用 `UiSettingsService.Load()` 获取 `_uiSettings`。
    - 创建 `MainViewModel` 时，将 `_uiSettings.ShowMarkdownHints` 赋值给 `MainViewModel.ShowMarkdownHints`。
  - 订阅 `MainViewModel.PropertyChanged`：
    - 当 `ShowMarkdownHints` 发生变化时：
      - 更新 `_uiSettings.ShowMarkdownHints`。
      - 调用 `UiSettingsService.Save(_uiSettings)` 立即持久化。
  - 效果：
    - 用户在界面中勾选/取消“显示 Markdown 语法提示”后，下次启动应用仍会保持相同设置。


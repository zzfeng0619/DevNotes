using System.Text;
using System.Windows;
using System.Windows.Controls;
using DevNotes.App.ViewModels;
using DevNotes.Infrastructure;
using Markdig;

namespace DevNotes.App;

/// <summary>
/// 应用主窗口的代码隐藏类，负责初始化 UI 以及承载 DataContext。
/// 在当前阶段，这里直接构造 ViewModel 和仓储对象，后续可替换为完整的依赖注入容器。
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private UiSettings _uiSettings = new();

    /// <summary>
    /// 初始化主窗口并加载 XAML 定义的界面。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // 先加载本地 UI 设置以还原用户偏好。
        _uiSettings = UiSettingsService.Load();

        // 初始化 SQLite 仓储和主视图模型，并设置为 DataContext。
        _viewModel = new MainViewModel(new SqliteNoteRepository())
        {
            ShowMarkdownHints = _uiSettings.ShowMarkdownHints
        };

        DataContext = _viewModel;

        // 当选中笔记变化或 UI 设置变化时，做相应处理。
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.SelectedNote))
            {
                UpdateMarkdownPreview(_viewModel.SelectedNote?.Content ?? string.Empty);
            }
            else if (args.PropertyName == nameof(MainViewModel.ShowMarkdownHints))
            {
                _uiSettings.ShowMarkdownHints = _viewModel.ShowMarkdownHints;
                UiSettingsService.Save(_uiSettings);
            }
        };

        // 窗口初次加载时，根据当前内容初始化预览。
        Loaded += (_, _) =>
        {
            UpdateMarkdownPreview(_viewModel.SelectedNote?.Content ?? string.Empty);
        };
    }

    /// <summary>
    /// 使用 Markdig 将 Markdown 文本渲染为 HTML，并展示在预览浏览器中。
    /// </summary>
    /// <param name="markdown">要渲染的 Markdown 文本。</param>
    private void UpdateMarkdownPreview(string markdown)
    {
        if (MarkdownPreviewBrowser == null)
        {
            return;
        }

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        var htmlBody = Markdig.Markdown.ToHtml(markdown ?? string.Empty, pipeline);

        var html = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html>")
            .AppendLine("<head>")
            .AppendLine("<meta charset=\"utf-8\" />")
            .AppendLine("<style>")
            .AppendLine("body { font-family: 'Segoe UI', sans-serif; margin: 12px; }")
            .AppendLine("pre { background-color: #f5f5f5; padding: 8px; overflow-x: auto; }")
            .AppendLine("code { font-family: Consolas, monospace; }")
            .AppendLine("</style>")
            .AppendLine("</head>")
            .AppendLine("<body>")
            .AppendLine(htmlBody)
            .AppendLine("</body>")
            .AppendLine("</html>")
            .ToString();

        MarkdownPreviewBrowser.NavigateToString(html);
    }

    /// <summary>
    /// 编辑区域内容发生变化时触发，实时刷新 Markdown 预览。
    /// </summary>
    private void ContentTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel?.SelectedNote == null)
        {
            return;
        }

        var currentText = ContentTextBox.Text ?? string.Empty;
        UpdateMarkdownPreview(currentText);
    }

    /// <summary>
    /// 在当前光标处插入指定 Markdown 片段，或对选中文本进行包装。
    /// </summary>
    /// <param name="before">选中文本前缀片段。</param>
    /// <param name="after">选中文本后缀片段。</param>
    /// <param name="placeholder">当未选中文本时插入的占位内容。</param>
    private void InsertMarkdownSnippet(string before, string after, string placeholder)
    {
        if (ContentTextBox == null)
        {
            return;
        }

        var selectionStart = ContentTextBox.SelectionStart;
        var selectionLength = ContentTextBox.SelectionLength;
        var text = ContentTextBox.Text ?? string.Empty;

        if (selectionLength > 0)
        {
            var selectedText = text.Substring(selectionStart, selectionLength);
            var newText = text.Remove(selectionStart, selectionLength)
                              .Insert(selectionStart, $"{before}{selectedText}{after}");

            ContentTextBox.Text = newText;
            ContentTextBox.SelectionStart = selectionStart + before.Length + selectedText.Length + after.Length;
            ContentTextBox.SelectionLength = 0;
        }
        else
        {
            var newText = text.Insert(selectionStart, $"{before}{placeholder}{after}");
            ContentTextBox.Text = newText;
            ContentTextBox.SelectionStart = selectionStart + before.Length;
            ContentTextBox.SelectionLength = placeholder.Length;
        }

        UpdateMarkdownPreview(ContentTextBox.Text ?? string.Empty);
    }

    /// <summary>
    /// 插入一级标题语法。
    /// </summary>
    private void HeadingButton_OnClick(object sender, RoutedEventArgs e)
    {
        InsertMarkdownSnippet("# ", string.Empty, "标题");
    }

    /// <summary>
    /// 插入加粗语法或包裹当前选中文本。
    /// </summary>
    private void BoldButton_OnClick(object sender, RoutedEventArgs e)
    {
        InsertMarkdownSnippet("**", "**", "加粗文本");
    }

    /// <summary>
    /// 插入代码块语法。
    /// </summary>
    private void CodeBlockButton_OnClick(object sender, RoutedEventArgs e)
    {
        InsertMarkdownSnippet("```csharp\n", "\n```", "// 代码内容");
    }

    /// <summary>
    /// 插入列表项语法。
    /// </summary>
    private void ListItemButton_OnClick(object sender, RoutedEventArgs e)
    {
        InsertMarkdownSnippet("- ", string.Empty, "列表项");
    }
}
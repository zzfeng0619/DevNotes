using System.Text;
using System.Windows;
using System.Windows.Controls;
using DevNotes.App.ViewModels;
using DevNotes.Infrastructure;
using DevNotes.Infrastructure.Repositories;
using Markdig;

namespace DevNotes.App;

/// <summary>
/// 应用主窗口的代码隐藏类，负责初始化 UI 以及承载 DataContext。
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private AppSettings _settings = new();

    /// <summary>
    /// 初始化主窗口并加载 XAML 定义的界面。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // 加载设置
        _settings = SettingsService.Load();

        // 初始化数据库
        DatabaseInitializer.EnsureDatabaseCreated();
        var connectionString = DatabaseInitializer.GetConnectionString();

        // 创建仓储实例
        var articleRepository = new SqliteArticleRepository(connectionString);
        var categoryRepository = new SqliteCategoryRepository(connectionString);
        var tagRepository = new SqliteTagRepository(connectionString);

        // 初始化 ViewModel
        _viewModel = new MainViewModel(articleRepository, categoryRepository, tagRepository)
        {
            ShowMarkdownHints = _settings.ShowMarkdownHints
        };

        DataContext = _viewModel;

        // 监听属性变化
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.SelectedArticle))
            {
                UpdateMarkdownPreview(_viewModel.SelectedArticle?.Content ?? string.Empty);
            }
            else if (args.PropertyName == nameof(MainViewModel.ShowMarkdownHints))
            {
                _settings.ShowMarkdownHints = _viewModel.ShowMarkdownHints;
                SettingsService.Save(_settings);
            }
        };

        // 窗口初次加载时初始化预览
        Loaded += (_, _) =>
        {
            UpdateMarkdownPreview(_viewModel.SelectedArticle?.Content ?? string.Empty);
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
            .AppendLine("pre { background-color: #f5f5f5; padding: 8px; overflow-x: auto; border-radius: 4px; }")
            .AppendLine("code { font-family: Consolas, monospace; background-color: #f0f0f0; padding: 2px 4px; border-radius: 2px; }")
            .AppendLine("pre code { background-color: transparent; padding: 0; }")
            .AppendLine("blockquote { border-left: 3px solid #ddd; margin: 0; padding-left: 12px; color: #666; }")
            .AppendLine("img { max-width: 100%; height: auto; }")
            .AppendLine("table { border-collapse: collapse; width: 100%; }")
            .AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }")
            .AppendLine("th { background-color: #f5f5f5; }")
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
        if (_viewModel?.SelectedArticle == null)
        {
            return;
        }

        var currentText = ContentTextBox.Text ?? string.Empty;
        UpdateMarkdownPreview(currentText);
    }

    /// <summary>
    /// 在当前光标处插入指定 Markdown 片段，或对选中文本进行包装。
    /// </summary>
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

    /// <summary>
    /// 插入图片语法。
    /// </summary>
    private void ImageButton_OnClick(object sender, RoutedEventArgs e)
    {
        InsertMarkdownSnippet("![", "](图片路径)", "图片描述");
    }

    /// <summary>
    /// 全部筛选点击事件。
    /// </summary>
    private void FilterAll_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.FilterAll = true;
        }
    }

    /// <summary>
    /// 草稿筛选点击事件。
    /// </summary>
    private void FilterDraft_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.FilterDraft = true;
        }
    }

    /// <summary>
    /// 已发布筛选点击事件。
    /// </summary>
    private void FilterPublished_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.FilterPublished = true;
        }
    }
}

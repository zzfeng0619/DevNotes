using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevNotes.App.ViewModels;
using DevNotes.App.Views;
using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;
using DevNotes.Infrastructure;
using DevNotes.Infrastructure.Repositories;
using DevNotes.Infrastructure.Services;
using Markdig;
using Microsoft.Win32;

namespace DevNotes.App;

/// <summary>
/// 应用主窗口的代码隐藏类，负责初始化 UI 以及承载 DataContext。
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private AppSettings _settings = new();
    private IImageStorageService _imageStorageService = null!;
    private IImageAttachmentRepository _imageAttachmentRepository = null!;
    private int? _currentArticleId;

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
        _imageAttachmentRepository = new SqliteImageAttachmentRepository(connectionString);
        _imageStorageService = new LocalImageStorageService();

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
                _currentArticleId = _viewModel.SelectedArticle?.Id;
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
            _currentArticleId = _viewModel.SelectedArticle?.Id;
        };

        // 注册剪贴板粘贴事件
        ContentTextBox.PreviewKeyDown += ContentTextBox_PreviewKeyDown;
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

        // 将相对图片路径转换为 Base64 Data URL
        var imagesDir = AppDataPaths.GetImagesDirectory();
        htmlBody = Regex.Replace(
            htmlBody,
            @"src=""([^""]+)""",
            match =>
            {
                var relativePath = match.Groups[1].Value;
                if (!relativePath.StartsWith("http") && !relativePath.StartsWith("data:"))
                {
                    var absolutePath = Path.Combine(imagesDir, relativePath);
                    if (File.Exists(absolutePath))
                    {
                        try
                        {
                            var imageBytes = File.ReadAllBytes(absolutePath);
                            var base64 = Convert.ToBase64String(imageBytes);
                            var mimeType = GetMimeType(relativePath);
                            return $"src=\"data:{mimeType};base64,{base64}\"";
                        }
                        catch
                        {
                            // 如果读取失败，保持原路径
                        }
                    }
                }
                return match.Value;
            });

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
    /// 根据文件扩展名获取 MIME 类型。
    /// </summary>
    private static string GetMimeType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "image/png"
        };
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
    /// 键盘事件处理，用于捕获 Ctrl+V 粘贴图片。
    /// </summary>
    private void ContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (Clipboard.ContainsImage())
            {
                PasteImageFromClipboard();
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// 从剪贴板粘贴图片。
    /// </summary>
    private void PasteImageFromClipboard()
    {
        if (_currentArticleId == null || _currentArticleId == 0)
        {
            MessageBox.Show("请先保存文章后再粘贴图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var image = Clipboard.GetImage();
            if (image == null)
            {
                return;
            }

            // 将图片转换为 PNG 格式的字节数组
            using var stream = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);
            var imageData = stream.ToArray();

            // 保存图片
            SaveAndInsertImage(imageData, "pasted_image.png", "粘贴的图片");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"粘贴图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
    /// 保存图片并插入到编辑器。
    /// </summary>
    private void SaveAndInsertImage(byte[] imageData, string originalFileName, string altText)
    {
        if (_currentArticleId == null || _currentArticleId == 0)
        {
            MessageBox.Show("请先保存文章后再上传图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // 保存图片到本地
            var imageAttachment = _imageStorageService.SaveImage(originalFileName, imageData);
            imageAttachment.ArticleId = _currentArticleId.Value;
            imageAttachment.AltText = altText;

            // 保存到数据库
            _imageAttachmentRepository.Add(imageAttachment);

            // 插入 Markdown 图片语法
            var imageMarkdown = $"![{altText}]({imageAttachment.StoragePath})";
            InsertMarkdownSnippet(imageMarkdown, string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
    /// 插入图片（打开上传对话框）。
    /// </summary>
    private void ImageButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentArticleId == null || _currentArticleId == 0)
        {
            MessageBox.Show("请先保存文章后再上传图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new ImageUploadDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.ImageData != null)
        {
            SaveAndInsertImage(dialog.ImageData, dialog.OriginalFileName, dialog.AltText);
        }
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

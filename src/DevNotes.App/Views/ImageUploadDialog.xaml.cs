using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DevNotes.App.Views;

/// <summary>
/// 图片上传对话框。
/// </summary>
public partial class ImageUploadDialog : Window
{
    private byte[]? _imageData;
    private string _originalFileName = string.Empty;

    /// <summary>
    /// 获取上传的图片数据。
    /// </summary>
    public byte[]? ImageData => _imageData;

    /// <summary>
    /// 获取原始文件名。
    /// </summary>
    public string OriginalFileName => _originalFileName;

    /// <summary>
    /// 获取图片描述。
    /// </summary>
    public string AltText => AltTextBox.Text ?? string.Empty;

    public ImageUploadDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 选择文件按钮点击事件。
    /// </summary>
    private void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择图片",
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp|所有文件|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadImage(dialog.FileName);
        }
    }

    /// <summary>
    /// 拖拽进入事件。
    /// </summary>
    private void Border_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DropZone.Background = System.Windows.Media.Brushes.LightBlue;
        }
    }

    /// <summary>
    /// 拖拽离开事件。
    /// </summary>
    private void Border_DragLeave(object sender, DragEventArgs e)
    {
        DropZone.Background = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F9F9F9"));
    }

    /// <summary>
    /// 拖拽放下事件。
    /// </summary>
    private void Border_Drop(object sender, DragEventArgs e)
    {
        DropZone.Background = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F9F9F9"));

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadImage(files[0]);
            }
        }
    }

    /// <summary>
    /// 加载图片并显示预览。
    /// </summary>
    private void LoadImage(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var validExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp" };

            if (!validExtensions.Contains(extension))
            {
                MessageBox.Show("不支持的图片格式", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _imageData = File.ReadAllBytes(filePath);
            _originalFileName = Path.GetFileName(filePath);

            // 显示预览
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();

            PreviewImage.Source = bitmap;
            PreviewImage.Visibility = Visibility.Visible;
            PlaceholderPanel.Visibility = Visibility.Collapsed;
            InsertButton.IsEnabled = true;

            // 自动设置 Alt Text
            if (string.IsNullOrWhiteSpace(AltTextBox.Text))
            {
                AltTextBox.Text = Path.GetFileNameWithoutExtension(filePath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 取消按钮点击事件。
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// 插入按钮点击事件。
    /// </summary>
    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (_imageData == null)
        {
            MessageBox.Show("请先选择一张图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
        Close();
    }
}

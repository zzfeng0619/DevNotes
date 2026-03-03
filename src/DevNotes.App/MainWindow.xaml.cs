using System.Windows;

namespace DevNotes.App;

/// <summary>
/// 应用主窗口的代码隐藏类，负责初始化 UI 以及承载 DataContext。
/// 大部分界面逻辑放在对应的 ViewModel 中，本类保持尽量精简。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 初始化主窗口并加载 XAML 定义的界面。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }
}
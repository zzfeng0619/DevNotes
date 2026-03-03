using System.Windows;
using DevNotes.App.ViewModels;
using DevNotes.Infrastructure;

namespace DevNotes.App;

/// <summary>
/// 应用主窗口的代码隐藏类，负责初始化 UI 以及承载 DataContext。
/// 在当前阶段，这里直接构造 ViewModel 和仓储对象，后续可替换为完整的依赖注入容器。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 初始化主窗口并加载 XAML 定义的界面。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // 初始化 SQLite 仓储和主视图模型，并设置为 DataContext。
        var noteRepository = new SqliteNoteRepository();
        DataContext = new MainViewModel(noteRepository);
    }
}
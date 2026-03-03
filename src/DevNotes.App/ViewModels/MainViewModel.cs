using System.Collections.ObjectModel;
using System.Windows.Input;
using DevNotes.Core;
using DevNotes.Domain;

namespace DevNotes.App.ViewModels;

/// <summary>
/// 应用主窗口对应的 ViewModel，
/// 负责管理笔记列表、当前选中笔记以及新建/保存/删除等基础操作。
/// </summary>
public class MainViewModel : ObservableObject
{
    private Note? _selectedNote;

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 实例，
    /// 并创建内存中的笔记集合（后续可替换为数据库加载）。
    /// </summary>
    public MainViewModel()
    {
        Notes = new ObservableCollection<Note>();

        NewNoteCommand = new RelayCommand(_ => CreateNewNote());
        SaveNoteCommand = new RelayCommand(_ => SaveCurrentNote(), _ => CanSaveCurrentNote());
        DeleteNoteCommand = new RelayCommand(_ => DeleteCurrentNote(), _ => SelectedNote != null);
    }

    /// <summary>
    /// 当前所有笔记的集合，用于绑定到左侧列表。
    /// </summary>
    public ObservableCollection<Note> Notes { get; }

    /// <summary>
    /// 当前在编辑或选中的笔记。
    /// </summary>
    public Note? SelectedNote
    {
        get => _selectedNote;
        set
        {
            if (SetProperty(ref _selectedNote, value))
            {
                // 当选中项变化时，删除命令的可执行状态也会随之变化。
                (DeleteNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (SaveNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 新建笔记命令，会在内存中创建一条新的笔记并设为当前选中项。
    /// </summary>
    public ICommand NewNoteCommand { get; }

    /// <summary>
    /// 保存当前笔记命令，在当前阶段主要用于刷新时间等基础信息。
    /// 后续接入数据库后，可以在此位置调用持久化服务。
    /// </summary>
    public ICommand SaveNoteCommand { get; }

    /// <summary>
    /// 删除当前选中笔记命令，在当前阶段为直接从集合中移除。
    /// 后续可调整为逻辑删除（回收站）。
    /// </summary>
    public ICommand DeleteNoteCommand { get; }

    /// <summary>
    /// 创建一条新的空白笔记并添加到集合中，同时设为当前选中笔记。
    /// </summary>
    private void CreateNewNote()
    {
        var note = new Note
        {
            Title = "未命名笔记",
            Content = string.Empty
        };

        Notes.Add(note);
        SelectedNote = note;
    }

    /// <summary>
    /// 保存当前笔记的数据。
    /// 当前实现仅更新更新时间，后续会扩展为落地到数据库。
    /// </summary>
    private void SaveCurrentNote()
    {
        if (SelectedNote == null)
        {
            return;
        }

        SelectedNote.UpdatedAt = DateTime.Now;
        (SaveNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// 删除当前选中笔记（直接从集合中移除）。
    /// </summary>
    private void DeleteCurrentNote()
    {
        if (SelectedNote == null)
        {
            return;
        }

        var noteToRemove = SelectedNote;
        Notes.Remove(noteToRemove);
        SelectedNote = Notes.FirstOrDefault();
    }

    /// <summary>
    /// 判断当前笔记是否可以执行保存操作。
    /// 可以根据标题或内容是否为空等条件进行约束。
    /// </summary>
    /// <returns>当存在选中笔记时返回 true，否则返回 false。</returns>
    private bool CanSaveCurrentNote()
    {
        return SelectedNote != null;
    }
}


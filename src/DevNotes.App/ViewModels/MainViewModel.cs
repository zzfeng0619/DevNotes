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
    private readonly INoteRepository _noteRepository;
    private Note? _selectedNote;

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 实例，
    /// 并通过仓储加载已有笔记列表。
    /// </summary>
    /// <param name="noteRepository">笔记数据访问仓储实现。</param>
    public MainViewModel(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;

        var existingNotes = _noteRepository.GetAll();
        Notes = new ObservableCollection<Note>(existingNotes);

        NewNoteCommand = new RelayCommand(_ => CreateNewNote());
        SaveNoteCommand = new RelayCommand(_ => SaveCurrentNote(), _ => CanSaveCurrentNote());
        DeleteNoteCommand = new RelayCommand(_ => DeleteCurrentNote(), _ => SelectedNote != null);

        SelectedNote = Notes.FirstOrDefault();
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
                (DeleteNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (SaveNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 新建笔记命令，会在集合和数据库中创建一条新的笔记并设为当前选中项。
    /// </summary>
    public ICommand NewNoteCommand { get; }

    /// <summary>
    /// 保存当前笔记命令，将变更同步到数据库中。
    /// </summary>
    public ICommand SaveNoteCommand { get; }

    /// <summary>
    /// 删除当前选中笔记命令，会在数据库中执行逻辑删除并从集合中移除。
    /// </summary>
    public ICommand DeleteNoteCommand { get; }

    /// <summary>
    /// 创建一条新的空白笔记并添加到集合与数据库中，同时设为当前选中笔记。
    /// </summary>
    private void CreateNewNote()
    {
        var now = DateTime.Now;
        var note = new Note
        {
            Title = "未命名笔记",
            Content = string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        _noteRepository.Add(note);
        Notes.Insert(0, note);
        SelectedNote = note;
    }

    /// <summary>
    /// 保存当前笔记的数据，并更新数据库中的对应记录。
    /// </summary>
    private void SaveCurrentNote()
    {
        if (SelectedNote == null)
        {
            return;
        }

        SelectedNote.UpdatedAt = DateTime.Now;
        _noteRepository.Update(SelectedNote);
        (SaveNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// 删除当前选中笔记（逻辑删除并从当前集合中移除）。
    /// </summary>
    private void DeleteCurrentNote()
    {
        if (SelectedNote == null)
        {
            return;
        }

        var noteToRemove = SelectedNote;
        _noteRepository.Delete(noteToRemove.Id);
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


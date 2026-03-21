using System.Collections.ObjectModel;
using System.Windows.Input;
using DevNotes.Core;
using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;

namespace DevNotes.App.ViewModels;

/// <summary>
/// 应用主窗口对应的 ViewModel，负责管理文章列表、分类、标签以及文章操作。
/// </summary>
public class MainViewModel : ObservableObject
{
    private readonly IArticleRepository _articleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;

    private Article? _selectedArticle;
    private Category? _filterCategory;
    private ArticleStatus? _filterStatus;
    private string _searchKeyword = string.Empty;
    private bool _showMarkdownHints;

    private bool _filterAll = true;
    private bool _filterDraft;
    private bool _filterPublished;

    /// <summary>
    /// 初始化 MainViewModel 实例。
    /// </summary>
    public MainViewModel(
        IArticleRepository articleRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository)
    {
        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;

        // 加载数据
        Articles = new ObservableCollection<Article>(_articleRepository.GetAll());
        Categories = new ObservableCollection<Category>(_categoryRepository.GetAll());
        Tags = new ObservableCollection<Tag>(_tagRepository.GetAll());

        // 初始化命令
        NewArticleCommand = new RelayCommand(_ => CreateNewArticle());
        SaveArticleCommand = new RelayCommand(_ => SaveCurrentArticle(), _ => CanSaveArticle());
        DeleteArticleCommand = new RelayCommand(_ => DeleteCurrentArticle(), _ => SelectedArticle != null);
        PublishArticleCommand = new RelayCommand(_ => PublishCurrentArticle(), _ => SelectedArticle?.Status == ArticleStatus.Draft);

        // 选中第一篇文章
        SelectedArticle = Articles.FirstOrDefault();
    }

    /// <summary>
    /// 文章列表。
    /// </summary>
    public ObservableCollection<Article> Articles { get; }

    /// <summary>
    /// 分类列表。
    /// </summary>
    public ObservableCollection<Category> Categories { get; }

    /// <summary>
    /// 标签列表。
    /// </summary>
    public ObservableCollection<Tag> Tags { get; }

    /// <summary>
    /// 当前选中的文章。
    /// </summary>
    public Article? SelectedArticle
    {
        get => _selectedArticle;
        set
        {
            if (SetProperty(ref _selectedArticle, value))
            {
                OnSelectedArticleChanged();
            }
        }
    }

    /// <summary>
    /// 分类筛选。
    /// </summary>
    public Category? FilterCategory
    {
        get => _filterCategory;
        set
        {
            if (SetProperty(ref _filterCategory, value))
            {
                FilterArticles();
            }
        }
    }

    /// <summary>
    /// 状态筛选。
    /// </summary>
    public ArticleStatus? FilterStatus
    {
        get => _filterStatus;
        set
        {
            if (SetProperty(ref _filterStatus, value))
            {
                FilterArticles();
            }
        }
    }

    /// <summary>
    /// 搜索关键词。
    /// </summary>
    public string SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            if (SetProperty(ref _searchKeyword, value))
            {
                FilterArticles();
            }
        }
    }

    /// <summary>
    /// 是否显示 Markdown 语法提示。
    /// </summary>
    public bool ShowMarkdownHints
    {
        get => _showMarkdownHints;
        set => SetProperty(ref _showMarkdownHints, value);
    }

    /// <summary>
    /// 全部筛选状态。
    /// </summary>
    public bool FilterAll
    {
        get => _filterAll;
        set
        {
            if (SetProperty(ref _filterAll, value) && value)
            {
                _filterDraft = false;
                _filterPublished = false;
                OnPropertyChanged(nameof(FilterDraft));
                OnPropertyChanged(nameof(FilterPublished));
                FilterStatus = null;
            }
        }
    }

    /// <summary>
    /// 草稿筛选状态。
    /// </summary>
    public bool FilterDraft
    {
        get => _filterDraft;
        set
        {
            if (SetProperty(ref _filterDraft, value) && value)
            {
                _filterAll = false;
                _filterPublished = false;
                OnPropertyChanged(nameof(FilterAll));
                OnPropertyChanged(nameof(FilterPublished));
                FilterStatus = ArticleStatus.Draft;
            }
        }
    }

    /// <summary>
    /// 已发布筛选状态。
    /// </summary>
    public bool FilterPublished
    {
        get => _filterPublished;
        set
        {
            if (SetProperty(ref _filterPublished, value) && value)
            {
                _filterAll = false;
                _filterDraft = false;
                OnPropertyChanged(nameof(FilterAll));
                OnPropertyChanged(nameof(FilterDraft));
                FilterStatus = ArticleStatus.Published;
            }
        }
    }

    /// <summary>
    /// 新建文章命令。
    /// </summary>
    public ICommand NewArticleCommand { get; }

    /// <summary>
    /// 保存文章命令。
    /// </summary>
    public ICommand SaveArticleCommand { get; }

    /// <summary>
    /// 删除文章命令。
    /// </summary>
    public ICommand DeleteArticleCommand { get; }

    /// <summary>
    /// 发布文章命令。
    /// </summary>
    public ICommand PublishArticleCommand { get; }

    /// <summary>
    /// 创建新文章。
    /// </summary>
    private void CreateNewArticle()
    {
        var now = DateTime.Now;
        var article = new Article
        {
            Title = "新文章",
            Content = string.Empty,
            Summary = string.Empty,
            Status = ArticleStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        article.GenerateSlug();
        _articleRepository.Add(article);

        // 根据当前筛选状态决定是否添加到列表
        if (FilterStatus == null || FilterStatus == ArticleStatus.Draft)
        {
            Articles.Insert(0, article);
        }

        SelectedArticle = article;
    }

    /// <summary>
    /// 保存当前文章。
    /// </summary>
    private void SaveCurrentArticle()
    {
        if (SelectedArticle == null)
        {
            return;
        }

        SelectedArticle.UpdatedAt = DateTime.Now;
        SelectedArticle.GenerateSlug();
        _articleRepository.Update(SelectedArticle);

        RefreshCommands();
    }

    /// <summary>
    /// 删除当前文章。
    /// </summary>
    private void DeleteCurrentArticle()
    {
        if (SelectedArticle == null)
        {
            return;
        }

        var articleToRemove = SelectedArticle;
        _articleRepository.Delete(articleToRemove.Id);
        Articles.Remove(articleToRemove);
        SelectedArticle = Articles.FirstOrDefault();
    }

    /// <summary>
    /// 发布当前文章。
    /// </summary>
    private void PublishCurrentArticle()
    {
        if (SelectedArticle == null || SelectedArticle.Status != ArticleStatus.Draft)
        {
            return;
        }

        var now = DateTime.Now;
        SelectedArticle.Status = ArticleStatus.Published;
        SelectedArticle.PublishedAt = now;
        SelectedArticle.UpdatedAt = now;

        _articleRepository.Update(SelectedArticle);

        // 如果当前筛选是草稿，从列表中移除
        if (FilterStatus == ArticleStatus.Draft)
        {
            Articles.Remove(SelectedArticle);
            SelectedArticle = Articles.FirstOrDefault();
        }

        RefreshCommands();
    }

    /// <summary>
    /// 筛选文章列表。
    /// </summary>
    private void FilterArticles()
    {
        Articles.Clear();

        var allArticles = _articleRepository.GetAll(FilterStatus);

        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            allArticles = _articleRepository.Search(SearchKeyword)
                .Where(a => FilterStatus == null || a.Status == FilterStatus)
                .ToList();
        }

        if (FilterCategory != null)
        {
            allArticles = allArticles.Where(a => a.CategoryId == FilterCategory.Id).ToList();
        }

        foreach (var article in allArticles)
        {
            Articles.Add(article);
        }

        SelectedArticle = Articles.FirstOrDefault();
    }

    /// <summary>
    /// 选中文章变化时的处理。
    /// </summary>
    private void OnSelectedArticleChanged()
    {
        // 更新关联的标签
        if (SelectedArticle != null)
        {
            SelectedArticle.Tags = new List<Tag>(_tagRepository.GetByArticle(SelectedArticle.Id));
        }

        RefreshCommands();
    }

    /// <summary>
    /// 更新命令可用状态。
    /// </summary>
    private void RefreshCommands()
    {
        (SaveArticleCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteArticleCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PublishArticleCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// 判断是否可以保存文章。
    /// </summary>
    private bool CanSaveArticle()
    {
        return SelectedArticle != null;
    }
}

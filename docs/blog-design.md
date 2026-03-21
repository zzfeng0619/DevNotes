# DevBlog 个人博客系统 - 开发设计文档

## 1. 项目概述

### 1.1 目标

将现有的 DevNotes 笔记工具改造为**个人博客系统**，核心功能：

- Markdown 文档编写与实时预览
- 本地图片上传与管理
- 代码块语法高亮
- 文章分类与标签管理
- 静态站点导出（可选）

### 1.2 技术栈

| 组件 | 技术 |
|------|------|
| 框架 | WPF + .NET 10 |
| 数据库 | SQLite |
| Markdown 渲染 | Markdig |
| 语法高亮 | ColorCode / Highlight.js（预览时） |
| 图片存储 | 本地文件系统 |

---

## 2. 架构设计

### 2.1 分层结构

```
DevBlog.App              # WPF UI 层
├── ViewModels/          # ViewModel
├── Converters/          # 值转换器
├── Controls/            # 自定义控件
└── Services/            # 应用服务

DevBlog.Domain           # 领域模型与仓储接口
├── Models/              # 实体模型
└── Interfaces/          # 仓储/服务接口

DevBlog.Infrastructure   # 基础设施层
├── Repositories/        # SQLite 仓储实现
├── Storage/             # 文件存储服务
└── Services/            # 导出服务等

DevBlog.Core             # 通用基类与工具
├── MVVM/                # ObservableObject, RelayCommand
└── Helpers/             # 工具方法

DevBlog.Tests            # 单元测试
```

### 2.2 数据存储结构

```
data/
├── db/
│   └── blog.db          # SQLite 主数据库
├── images/
│   ├── 2026/03/         # 按年/月组织
│   │   ├── img_xxx.png
│   │   └── img_xxx.jpg
│   └── thumbnails/      # 缩略图缓存
├── attachments/         # 其他附件
└── config/
    └── settings.json    # 应用配置
```

---

## 3. 领域模型

### 3.1 Article（文章）

```csharp
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }               // 标题
    public string Slug { get; set; }                // URL 友好标识
    public string Content { get; set; }             // Markdown 正文
    public string Summary { get; set; }             // 摘要（自动生成或手动）
    public string CoverImagePath { get; set; }      // 封面图路径
    public ArticleStatus Status { get; set; }       // 草稿/已发布
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }      // 发布时间
    public bool IsDeleted { get; set; }             // 逻辑删除

    // 导航属性
    public Category Category { get; set; }
    public List<Tag> Tags { get; set; }
    public List<ImageAttachment> Images { get; set; }
}

public enum ArticleStatus
{
    Draft,          // 草稿
    Published       // 已发布
}
```

### 3.2 Category（分类）

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }                // 分类名称
    public string Slug { get; set; }                // URL 标识
    public string Description { get; set; }         // 分类描述
    public int SortOrder { get; set; }              // 排序权重
}
```

### 3.3 Tag（标签）

```csharp
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }                // 标签名称
    public string Slug { get; set; }                // URL 标识
    public int ArticleCount { get; set; }           // 文章数量（冗余）
}
```

### 3.4 ImageAttachment（图片附件）

```csharp
public class ImageAttachment
{
    public int Id { get; set; }
    public int ArticleId { get; set; }              // 所属文章
    public string OriginalFileName { get; set; }    // 原始文件名
    public string StoragePath { get; set; }         // 存储相对路径
    public string AltText { get; set; }             // 图片描述
    public long FileSize { get; set; }              // 文件大小（字节）
    public int Width { get; set; }                  // 图片宽度
    public int Height { get; set; }                 // 图片高度
    public DateTime CreatedAt { get; set; }
}
```

---

## 4. 数据库设计

### 4.1 表结构

```sql
-- 文章表
CREATE TABLE Articles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Slug TEXT NOT NULL UNIQUE,
    Content TEXT NOT NULL DEFAULT '',
    Summary TEXT NOT NULL DEFAULT '',
    CoverImagePath TEXT,
    Status INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    PublishedAt TEXT,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    CategoryId INTEGER,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- 分类表
CREATE TABLE Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Slug TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL DEFAULT '',
    SortOrder INTEGER NOT NULL DEFAULT 0
);

-- 标签表
CREATE TABLE Tags (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Slug TEXT NOT NULL UNIQUE
);

-- 文章-标签关联表
CREATE TABLE ArticleTags (
    ArticleId INTEGER NOT NULL,
    TagId INTEGER NOT NULL,
    PRIMARY KEY (ArticleId, TagId),
    FOREIGN KEY (ArticleId) REFERENCES Articles(Id),
    FOREIGN KEY (TagId) REFERENCES Tags(Id)
);

-- 图片附件表
CREATE TABLE ImageAttachments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ArticleId INTEGER NOT NULL,
    OriginalFileName TEXT NOT NULL,
    StoragePath TEXT NOT NULL,
    AltText TEXT NOT NULL DEFAULT '',
    FileSize INTEGER NOT NULL,
    Width INTEGER NOT NULL DEFAULT 0,
    Height INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ArticleId) REFERENCES Articles(Id)
);
```

### 4.2 索引

```sql
CREATE INDEX idx_articles_status ON Articles(Status, IsDeleted);
CREATE INDEX idx_articles_category ON Articles(CategoryId);
CREATE INDEX idx_articles_created ON Articles(CreatedAt);
CREATE INDEX idx_images_article ON ImageAttachments(ArticleId);
```

---

## 5. 核心功能模块

### 5.1 文章编辑器

| 功能 | 说明 |
|------|------|
| Markdown 编辑 | 支持 GFM 语法，实时预览 |
| 代码块高亮 | 支持多语言语法高亮（C#, Python, JS 等） |
| 图片粘贴 | Ctrl+V 粘贴剪贴板图片自动上传 |
| 图片拖拽 | 拖拽图片文件到编辑器自动插入 |
| 快捷按钮 | 标题、加粗、代码块、列表、图片等 |
| 自动保存 | 可配置间隔自动保存草稿 |

### 5.2 图片管理

| 功能 | 说明 |
|------|------|
| 图片上传 | 支持 PNG/JPG/GIF/WebP |
| 自动重命名 | 使用 GUID 避免文件名冲突 |
| 目录组织 | 按年/月自动创建目录 |
| 图片压缩 | 可选的自动压缩 |
| 缩略图 | 自动生成缩略图用于列表展示 |
| 图片预览 | 支持点击放大查看 |

### 5.3 分类与标签

| 功能 | 说明 |
|------|------|
| 分类管理 | 增删改查，拖拽排序 |
| 标签管理 | 增删改查，自动补全 |
| 文章关联 | 编辑时选择分类和标签 |
| 筛选 | 按分类/标签筛选文章列表 |

### 5.4 文章管理

| 功能 | 说明 |
|------|------|
| 文章列表 | 显示标题、状态、时间、分类 |
| 搜索 | 标题和内容全文搜索 |
| 排序 | 按时间/标题排序 |
| 批量操作 | 批量删除、批量发布 |
| 回收站 | 逻辑删除，支持恢复 |

---

## 6. UI 设计

### 6.1 主窗口布局

```
┌─────────────────────────────────────────────────────────────┐
│  工具栏: [新建] [保存] [发布] [删除] [搜索...]              │
├──────────┬──────────────────────────────────────────────────┤
│          │                                                  │
│  侧边栏  │              编辑区 / 内容区                     │
│          │                                                  │
│ - 全部   │  ┌────────────────────────────────────────────┐  │
│ - 草稿   │  │ 标题: [________________________]           │  │
│ - 已发布 │  │ 分类: [下拉选择]  标签: [标签选择器]       │  │
│          │  │ 封面: [选择图片...] [预览]                 │  │
│ 分类     │  ├────────────────────────────────────────────┤  │
│ - 技术   │  │           │                                │  │
│ - 生活   │  │  Markdown │        实时预览                │  │
│ - 随笔   │  │   编辑器   │                                │  │
│          │  │           │                                │  │
│ 标签     │  │           │                                │  │
│ [标签云] │  │           │                                │  │
│          │  └────────────────────────────────────────────┘  │
├──────────┴──────────────────────────────────────────────────┤
│  状态栏: 字数统计 | 最后保存时间                             │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 图片上传对话框

```
┌─────────────────────────────────────────┐
│            插入图片                       │
├─────────────────────────────────────────┤
│  [拖拽图片到此处 或 点击选择文件]         │
│                                         │
│  ┌─────────────────────────────────┐    │
│  │                                 │    │
│  │        图片预览区域              │    │
│  │                                 │    │
│  └─────────────────────────────────┘    │
│                                         │
│  描述: [________________________]       │
│  尺寸: [原始大小] [自定义: ___x___]     │
│                                         │
│        [取消]              [插入]       │
└─────────────────────────────────────────┘
```

---

## 7. 仓储接口

### 7.1 IArticleRepository

```csharp
public interface IArticleRepository
{
    IReadOnlyList<Article> GetAll(ArticleStatus? status = null);
    Article? GetById(int id);
    Article? GetBySlug(string slug);
    void Add(Article article);
    void Update(Article article);
    void Delete(int articleId);                     // 逻辑删除
    void Restore(int articleId);                    // 恢复
    IReadOnlyList<Article> Search(string keyword);  // 搜索
    IReadOnlyList<Article> GetByCategory(int categoryId);
    IReadOnlyList<Article> GetByTag(int tagId);
}
```

### 7.2 ICategoryRepository

```csharp
public interface ICategoryRepository
{
    IReadOnlyList<Category> GetAll();
    Category? GetById(int id);
    void Add(Category category);
    void Update(Category category);
    void Delete(int categoryId);
}
```

### 7.3 ITagRepository

```csharp
public interface ITagRepository
{
    IReadOnlyList<Tag> GetAll();
    Tag? GetById(int id);
    Tag? GetByName(string name);
    void Add(Tag tag);
    void Delete(int tagId);
    void UpdateArticleTags(int articleId, IEnumerable<int> tagIds);
    IReadOnlyList<Tag> GetByArticle(int articleId);
}
```

### 7.4 IImageStorageService

```csharp
public interface IImageStorageService
{
    /// <summary>
    /// 存储图片并返回相对路径
    /// </summary>
    ImageAttachment SaveImage(string originalFileName, byte[] imageData);

    /// <summary>
    /// 删除图片文件
    /// </summary>
    void DeleteImage(string storagePath);

    /// <summary>
    /// 获取图片完整路径
    /// </summary>
    string GetFullPath(string storagePath);

    /// <summary>
    /// 生成缩略图
    /// </summary>
    void GenerateThumbnail(string storagePath, int maxWidth = 200);
}
```

---

## 8. ViewModel 设计

### 8.1 MainViewModel

```csharp
public class MainViewModel : ObservableObject
{
    // 文章列表
    public ObservableCollection<Article> Articles { get; }

    // 分类列表
    public ObservableCollection<Category> Categories { get; }

    // 标签列表
    public ObservableCollection<Tag> Tags { get; }

    // 当前编辑的文章
    public Article? SelectedArticle { get; set; }

    // 筛选条件
    public ArticleStatus? FilterStatus { get; set; }
    public Category? FilterCategory { get; set; }
    public Tag? FilterTag { get; set; }
    public string SearchKeyword { get; set; }

    // 命令
    public ICommand NewArticleCommand { get; }
    public ICommand SaveArticleCommand { get; }
    public ICommand PublishArticleCommand { get; }
    public ICommand DeleteArticleCommand { get; }
    public ICommand InsertImageCommand { get; }
    public ICommand ExportCommand { get; }
}
```

### 8.2 ArticleEditorViewModel

```csharp
public class ArticleEditorViewModel : ObservableObject
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public Category? SelectedCategory { get; set; }
    public ObservableCollection<Tag> SelectedTags { get; }
    public string? CoverImagePath { get; set; }

    // Markdown 预览
    public string HtmlPreview { get; }

    // 统计信息
    public int WordCount { get; }
    public DateTime LastSavedAt { get; }

    public ICommand InsertImageCommand { get; }
    public ICommand InsertCodeBlockCommand { get; }
    public ICommand SaveCommand { get; }
}
```

---

## 9. 开发计划

### 第一阶段：基础框架改造（优先级：高）

- [ ] 重命名项目结构（DevNotes → DevBlog）
- [ ] 创建 Article、Category、Tag、ImageAttachment 模型
- [ ] 实现数据库迁移（创建新表结构）
- [ ] 实现 ArticleRepository、CategoryRepository、TagRepository

### 第二阶段：图片管理（优先级：高）

- [ ] 实现 ImageStorageService（本地文件存储）
- [ ] 实现图片上传对话框
- [ ] 支持剪贴板粘贴图片
- [ ] 支持拖拽上传图片
- [ ] 自动生成缩略图

### 第三阶段：编辑器增强（优先级：高）

- [ ] 集成代码语法高亮（ColorCode）
- [ ] 增强 Markdown 快捷按钮（图片、链接、表格等）
- [ ] 实现自动保存功能
- [ ] 优化预览渲染样式

### 第四阶段：文章管理（优先级：中）

- [ ] 分类管理界面
- [ ] 标签管理界面（含标签云）
- [ ] 文章筛选与搜索
- [ ] 回收站功能

### 第五阶段：导出与发布（优先级：低）

- [ ] 静态 HTML 导出
- [ ] 博客模板系统
- [ ] 生成站点地图

---

## 10. 从 DevNotes 迁移

### 10.1 保留的组件

| 组件 | 说明 |
|------|------|
| `ObservableObject` | MVVM 基类，保留 |
| `RelayCommand` | 命令绑定，保留 |
| `AppDataPaths` | 路径管理，扩展为博客路径 |
| `UiSettingsService` | 配置持久化，保留并扩展 |
| `SqliteNoteRepository` | 作为 ArticleRepository 基础 |

### 10.2 删除/替换的组件

| 组件 | 处理方式 |
|------|----------|
| `Note` 模型 | 替换为 `Article` |
| `INoteRepository` | 替换为 `IArticleRepository` |
| `MainViewModel` | 重构为博客管理 ViewModel |
| `MainWindow.xaml` | 重新设计布局 |

### 10.3 数据迁移（可选）

```sql
-- 将原有 Note 数据迁移到 Article
INSERT INTO Articles (Title, Content, CreatedAt, UpdatedAt, Status, Slug)
SELECT Title, Content, CreatedAt, UpdatedAt,
       CASE WHEN IsPinned = 1 THEN 1 ELSE 0 END,
       LOWER(REPLACE(TRIM(Title), ' ', '-'))
FROM Notes
WHERE IsDeleted = 0;
```

---

## 11. 依赖项

```xml
<PackageReference Include="Markdig" Version="1.1.0" />
<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.3800.47" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.*" />
<PackageReference Include="ColorCode.Core" Version="2.0.*" />
<PackageReference Include="System.Text.Json" Version="9.*" />
```

---

## 12. 测试策略

| 模块 | 测试类型 |
|------|----------|
| Repository | 集成测试（SQLite 内存库） |
| ImageStorage | 单元测试（Mock 文件系统） |
| ViewModel | 单元测试 |
| Markdown 渲染 | 单元测试（输入输出比对） |

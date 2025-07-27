using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IHelpArticleRepository : IRepository<HelpArticle>
    {
        Task<HelpArticle?> GetBySlugAsync(string slug);
        Task<IEnumerable<HelpArticle>> GetByCategoryAsync(HelpArticleCategory category);
        Task<IEnumerable<HelpArticle>> GetByTypeAsync(HelpArticleType type);
        Task<IEnumerable<HelpArticle>> GetFeaturedArticlesAsync();
        Task<IEnumerable<HelpArticle>> GetPublishedArticlesAsync();
        Task<IEnumerable<HelpArticle>> GetRecentArticlesAsync(int count);
        Task<IEnumerable<HelpArticle>> SearchArticlesAsync(string searchTerm);
        Task<IEnumerable<HelpArticle>> SearchArticlesAsync(string searchTerm, HelpArticleType? type, HelpArticleCategory? category);
        Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
        Task<bool> IncrementViewCountAsync(int id);
    }
}

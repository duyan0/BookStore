using BookStore.Core.DTOs;
using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IHelpArticleService
    {
        Task<IEnumerable<HelpArticleListDto>> GetAllArticlesAsync();
        Task<HelpArticleDto?> GetArticleByIdAsync(int id);
        Task<HelpArticleDto?> GetArticleBySlugAsync(string slug);
        Task<HelpArticleDto> CreateArticleAsync(CreateHelpArticleDto createArticleDto);
        Task<HelpArticleDto?> UpdateArticleAsync(int id, UpdateHelpArticleDto updateArticleDto);
        Task<bool> DeleteArticleAsync(int id);
        
        // Category and Type filtering
        Task<IEnumerable<HelpArticleListDto>> GetArticlesByCategoryAsync(HelpArticleCategory category);
        Task<IEnumerable<HelpArticleListDto>> GetArticlesByTypeAsync(HelpArticleType type);
        
        // Featured and published articles
        Task<IEnumerable<HelpArticleListDto>> GetFeaturedArticlesAsync();
        Task<IEnumerable<HelpArticleListDto>> GetPublishedArticlesAsync();
        Task<IEnumerable<HelpArticleListDto>> GetRecentArticlesAsync(int count = 10);
        
        // Search functionality
        Task<HelpSearchResultDto> SearchArticlesAsync(HelpSearchDto searchDto);
        
        // Home page data
        Task<HelpCenterHomeDto> GetHelpCenterHomeDataAsync();
        
        // View tracking
        Task<bool> IncrementViewCountAsync(int id);
        
        // Admin functions
        Task<bool> TogglePublishStatusAsync(int id);
        Task<bool> ToggleFeaturedStatusAsync(int id);
        Task<bool> UpdateDisplayOrderAsync(int id, int newOrder);
    }
}

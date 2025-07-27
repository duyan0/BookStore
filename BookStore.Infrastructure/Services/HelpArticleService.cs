using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using System.Text.RegularExpressions;

namespace BookStore.Infrastructure.Services
{
    public class HelpArticleService : IHelpArticleService
    {
        private readonly IHelpArticleRepository _helpArticleRepository;
        private readonly IMapper _mapper;

        public HelpArticleService(IHelpArticleRepository helpArticleRepository, IMapper mapper)
        {
            _helpArticleRepository = helpArticleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HelpArticleListDto>> GetAllArticlesAsync()
        {
            var articles = await _helpArticleRepository.GetPublishedArticlesAsync();
            return _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);
        }

        public async Task<HelpArticleDto?> GetArticleByIdAsync(int id)
        {
            var article = await _helpArticleRepository.GetByIdAsync(id);
            return article == null ? null : _mapper.Map<HelpArticleDto>(article);
        }

        public async Task<HelpArticleDto?> GetArticleBySlugAsync(string slug)
        {
            var article = await _helpArticleRepository.GetBySlugAsync(slug);
            if (article != null)
            {
                // Increment view count
                await _helpArticleRepository.IncrementViewCountAsync(article.Id);
            }
            return article == null ? null : _mapper.Map<HelpArticleDto>(article);
        }

        public async Task<HelpArticleDto> CreateArticleAsync(CreateHelpArticleDto createArticleDto)
        {
            var article = _mapper.Map<HelpArticle>(createArticleDto);
            
            // Generate slug if not provided
            if (string.IsNullOrEmpty(article.Slug))
            {
                article.Slug = GenerateSlug(article.Title);
            }

            // Ensure slug is unique
            var originalSlug = article.Slug;
            var counter = 1;
            while (!await _helpArticleRepository.IsSlugUniqueAsync(article.Slug))
            {
                article.Slug = $"{originalSlug}-{counter}";
                counter++;
            }

            var createdArticle = await _helpArticleRepository.AddAsync(article);
            return _mapper.Map<HelpArticleDto>(createdArticle);
        }

        public async Task<HelpArticleDto?> UpdateArticleAsync(int id, UpdateHelpArticleDto updateArticleDto)
        {
            var article = await _helpArticleRepository.GetByIdAsync(id);
            if (article == null) return null;

            _mapper.Map(updateArticleDto, article);
            
            // Update slug if title changed
            if (!string.IsNullOrEmpty(updateArticleDto.Title) && updateArticleDto.Title != article.Title)
            {
                var newSlug = GenerateSlug(updateArticleDto.Title);
                if (await _helpArticleRepository.IsSlugUniqueAsync(newSlug, id))
                {
                    article.Slug = newSlug;
                }
            }

            article.LastModifiedAt = DateTime.UtcNow;
            // Note: LastModifiedById should be set from current user context

            await _helpArticleRepository.UpdateAsync(article);
            return _mapper.Map<HelpArticleDto>(article);
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _helpArticleRepository.GetByIdAsync(id);
            if (article == null) return false;

            await _helpArticleRepository.DeleteAsync(article);
            return true;
        }

        public async Task<IEnumerable<HelpArticleListDto>> GetArticlesByCategoryAsync(HelpArticleCategory category)
        {
            var articles = await _helpArticleRepository.GetByCategoryAsync(category);
            return _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);
        }

        public async Task<IEnumerable<HelpArticleListDto>> GetArticlesByTypeAsync(HelpArticleType type)
        {
            var articles = await _helpArticleRepository.GetByTypeAsync(type);
            return _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);
        }

        public async Task<IEnumerable<HelpArticleListDto>> GetFeaturedArticlesAsync()
        {
            var articles = await _helpArticleRepository.GetFeaturedArticlesAsync();
            return _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);
        }

        public async Task<IEnumerable<HelpArticleListDto>> GetPublishedArticlesAsync()
        {
            var articles = await _helpArticleRepository.GetPublishedArticlesAsync();
            return _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);
        }

        public async Task<IEnumerable<HelpArticleListDto>> GetRecentArticlesAsync(int count = 10)
        {
            var articles = await _helpArticleRepository.GetRecentArticlesAsync(count);
            return _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);
        }

        public async Task<HelpSearchResultDto> SearchArticlesAsync(HelpSearchDto searchDto)
        {
            var articles = await _helpArticleRepository.SearchArticlesAsync(
                searchDto.Query, 
                searchDto.Type, 
                searchDto.Category);

            var articleDtos = _mapper.Map<IEnumerable<HelpArticleListDto>>(articles);

            return new HelpSearchResultDto
            {
                Articles = articleDtos.ToList(),
                TotalCount = articleDtos.Count(),
                Query = searchDto.Query
            };
        }

        public async Task<HelpCenterHomeDto> GetHelpCenterHomeDataAsync()
        {
            var featuredArticles = await GetFeaturedArticlesAsync();
            var recentArticles = await GetRecentArticlesAsync(5);
            var popularFAQs = await GetArticlesByTypeAsync(HelpArticleType.FAQ);

            // Get category statistics
            var categories = new List<HelpCategoryStatsDto>();
            foreach (HelpArticleCategory category in Enum.GetValues<HelpArticleCategory>())
            {
                var categoryArticles = await GetArticlesByCategoryAsync(category);
                categories.Add(new HelpCategoryStatsDto
                {
                    Category = category,
                    CategoryName = GetCategoryDisplayName(category),
                    ArticleCount = categoryArticles.Count()
                });
            }

            return new HelpCenterHomeDto
            {
                FeaturedArticles = featuredArticles.ToList(),
                RecentArticles = recentArticles.ToList(),
                PopularFAQs = popularFAQs.Take(10).ToList(),
                Categories = categories.Where(c => c.ArticleCount > 0).ToList()
            };
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            return await _helpArticleRepository.IncrementViewCountAsync(id);
        }

        public async Task<bool> TogglePublishStatusAsync(int id)
        {
            var article = await _helpArticleRepository.GetByIdAsync(id);
            if (article == null) return false;

            article.IsPublished = !article.IsPublished;
            await _helpArticleRepository.UpdateAsync(article);
            return true;
        }

        public async Task<bool> ToggleFeaturedStatusAsync(int id)
        {
            var article = await _helpArticleRepository.GetByIdAsync(id);
            if (article == null) return false;

            article.IsFeatured = !article.IsFeatured;
            await _helpArticleRepository.UpdateAsync(article);
            return true;
        }

        public async Task<bool> UpdateDisplayOrderAsync(int id, int newOrder)
        {
            var article = await _helpArticleRepository.GetByIdAsync(id);
            if (article == null) return false;

            article.DisplayOrder = newOrder;
            await _helpArticleRepository.UpdateAsync(article);
            return true;
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;

            // Convert to lowercase and replace spaces with hyphens
            var slug = title.ToLowerInvariant().Replace(" ", "-");
            
            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            // Remove multiple consecutive hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            
            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            return slug;
        }

        private static string GetCategoryDisplayName(HelpArticleCategory category)
        {
            return category switch
            {
                HelpArticleCategory.General => "Tổng quan",
                HelpArticleCategory.Account => "Tài khoản",
                HelpArticleCategory.Orders => "Đơn hàng",
                HelpArticleCategory.Payment => "Thanh toán",
                HelpArticleCategory.Shipping => "Vận chuyển",
                HelpArticleCategory.Returns => "Đổi trả",
                HelpArticleCategory.Technical => "Kỹ thuật",
                HelpArticleCategory.Privacy => "Bảo mật",
                HelpArticleCategory.Terms => "Điều khoản",
                HelpArticleCategory.Contact => "Liên hệ",
                _ => category.ToString()
            };
        }
    }
}

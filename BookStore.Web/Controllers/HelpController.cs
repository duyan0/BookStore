using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;

namespace BookStore.Web.Controllers
{
    public class HelpController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<HelpController> _logger;

        public HelpController(ApiService apiService, ILogger<HelpController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Help
        public async Task<IActionResult> Index()
        {
            try
            {
                var homeData = await _apiService.GetAsync<HelpCenterHomeDto>("help/home");
                var viewModel = MapToViewModel(homeData);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading help center home");
                return View(new HelpCenterHomeViewModel());
            }
        }

        // GET: Help/Article/slug
        public async Task<IActionResult> Article(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            try
            {
                var article = await _apiService.GetAsync<HelpArticleDto>($"help/article/{slug}");
                if (article == null)
                {
                    return NotFound();
                }

                var viewModel = MapToViewModel(article);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading help article: {Slug}", slug);
                return NotFound();
            }
        }

        // GET: Help/Category/general
        public async Task<IActionResult> Category(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return NotFound();
            }

            try
            {
                if (!Enum.TryParse<HelpArticleCategory>(category, true, out var categoryEnum))
                {
                    return NotFound();
                }

                var articles = await _apiService.GetAsync<List<HelpArticleListDto>>($"help/category/{category}");
                var viewModel = new HelpCategoryViewModel
                {
                    Category = categoryEnum,
                    CategoryName = GetCategoryDisplayName(categoryEnum),
                    Articles = articles?.Select(MapToListViewModel).ToList() ?? new List<HelpArticleListViewModel>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading help category: {Category}", category);
                return NotFound();
            }
        }

        // GET: Help/Search
        public async Task<IActionResult> Search(string q, string type, string category)
        {
            var viewModel = new HelpSearchViewModel
            {
                Query = q ?? string.Empty,
                Results = new List<HelpArticleListViewModel>()
            };

            if (!string.IsNullOrWhiteSpace(q))
            {
                try
                {
                    var searchDto = new HelpSearchDto
                    {
                        Query = q,
                        Type = !string.IsNullOrEmpty(type) && Enum.TryParse<HelpArticleType>(type, out var typeEnum) ? typeEnum : null,
                        Category = !string.IsNullOrEmpty(category) && Enum.TryParse<HelpArticleCategory>(category, out var catEnum) ? catEnum : null
                    };

                    var results = await _apiService.PostAsync<HelpSearchResultDto>("help/search", searchDto);
                    if (results != null)
                    {
                        viewModel.Results = results.Articles?.Select(MapToListViewModel).ToList() ?? new List<HelpArticleListViewModel>();
                        viewModel.TotalCount = results.TotalCount;
                        viewModel.HasResults = results.HasResults;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching help articles: {Query}", q);
                }
            }

            return View(viewModel);
        }

        // GET: Help/FAQ
        public async Task<IActionResult> FAQ()
        {
            try
            {
                var faqs = await _apiService.GetAsync<List<HelpArticleListDto>>("help/type/faq");
                var viewModel = new HelpFAQViewModel
                {
                    FAQs = faqs?.Select(MapToListViewModel).ToList() ?? new List<HelpArticleListViewModel>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQ");
                return View(new HelpFAQViewModel());
            }
        }

        // GET: Help/Contact
        public IActionResult Contact()
        {
            var viewModel = new ContactViewModel();
            return View(viewModel);
        }

        // POST: Help/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Here you would typically send an email or save to database
                // For now, just show success message
                TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong thời gian sớm nhất.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form");
                TempData["Error"] = "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        #region Helper Methods

        private static HelpCenterHomeViewModel MapToViewModel(HelpCenterHomeDto? dto)
        {
            if (dto == null) return new HelpCenterHomeViewModel();

            return new HelpCenterHomeViewModel
            {
                FeaturedArticles = dto.FeaturedArticles?.Select(MapToListViewModel).ToList() ?? new List<HelpArticleListViewModel>(),
                RecentArticles = dto.RecentArticles?.Select(MapToListViewModel).ToList() ?? new List<HelpArticleListViewModel>(),
                PopularFAQs = dto.PopularFAQs?.Select(MapToListViewModel).ToList() ?? new List<HelpArticleListViewModel>(),
                Categories = dto.Categories?.Select(MapToCategoryViewModel).ToList() ?? new List<HelpCategoryStatsViewModel>()
            };
        }

        private static HelpArticleViewModel MapToViewModel(HelpArticleDto dto)
        {
            return new HelpArticleViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Slug = dto.Slug,
                Content = dto.Content,
                Summary = dto.Summary,
                Type = dto.Type,
                Category = dto.Category,
                ViewCount = dto.ViewCount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                AuthorName = dto.AuthorName
            };
        }

        private static HelpArticleListViewModel MapToListViewModel(HelpArticleListDto dto)
        {
            return new HelpArticleListViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Slug = dto.Slug,
                Summary = dto.Summary,
                Type = dto.Type,
                Category = dto.Category,
                ViewCount = dto.ViewCount,
                CreatedAt = dto.CreatedAt
            };
        }

        private static HelpCategoryStatsViewModel MapToCategoryViewModel(HelpCategoryStatsDto dto)
        {
            return new HelpCategoryStatsViewModel
            {
                Category = dto.Category,
                CategoryName = dto.CategoryName,
                CategoryDescription = dto.CategoryDescription,
                ArticleCount = dto.ArticleCount,
                IconClass = dto.IconClass
            };
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
                _ => "Khác"
            };
        }

        #endregion
    }
}

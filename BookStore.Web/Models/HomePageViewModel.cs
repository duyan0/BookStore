using BookStore.Core.DTOs;

namespace BookStore.Web.Models
{
    public class HomePageViewModel
    {
        public List<SliderDto> Sliders { get; set; } = new List<SliderDto>();
        public List<BannerDto> PromotionalBanners { get; set; } = new List<BannerDto>();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public List<BookViewModel> BestSellerBooks { get; set; } = new List<BookViewModel>();
        public List<BookViewModel> FeaturedBooks { get; set; } = new List<BookViewModel>();
        public List<BookViewModel> NewBooks { get; set; } = new List<BookViewModel>();
        public bool IsLoggedIn { get; set; }
        public string? UserFullName { get; set; }
        public bool IsAdmin { get; set; }
    }
}

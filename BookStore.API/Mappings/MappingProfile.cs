using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;

namespace BookStore.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Book mappings
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : string.Empty))
                .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.FinalPrice))
                .ForMember(dest => dest.IsDiscountActive, opt => opt.MapFrom(src => src.HasActiveDiscount))
                .ForMember(dest => dest.TotalDiscountAmount, opt => opt.MapFrom(src => src.TotalSavings))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.OriginalPrice));
            CreateMap<CreateBookDto, Book>()
                .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.Price));
            CreateMap<UpdateBookDto, Book>()
                .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.Price));

            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // Author mappings
            CreateMap<Author, AuthorDto>();
            CreateMap<CreateAuthorDto, Author>();
            CreateMap<UpdateAuthorDto, Author>();

            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<RegisterUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : string.Empty));
            CreateMap<CreateOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();

            // OrderDetail mappings
            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : string.Empty))
                .ForMember(dest => dest.BookImageUrl, opt => opt.MapFrom(src => src.Book != null ? src.Book.ImageUrl : string.Empty));
            CreateMap<CreateOrderDetailDto, OrderDetail>();

            // Slider mappings
            CreateMap<Slider, SliderDto>();
            CreateMap<CreateSliderDto, Slider>();
            CreateMap<UpdateSliderDto, Slider>();

            // Banner mappings
            CreateMap<Banner, BannerDto>();
            CreateMap<CreateBannerDto, Banner>();
            CreateMap<UpdateBannerDto, Banner>();

            // Voucher mappings
            CreateMap<Voucher, VoucherDto>();
            CreateMap<CreateVoucherDto, Voucher>();
            CreateMap<UpdateVoucherDto, Voucher>();

            // Review mappings
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.ReviewedByAdminName, opt => opt.MapFrom(src => src.ReviewedByAdmin != null ? $"{src.ReviewedByAdmin.FirstName} {src.ReviewedByAdmin.LastName}".Trim() : string.Empty));
            CreateMap<CreateReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>();

            // HelpArticle mappings
            CreateMap<HelpArticle, HelpArticleDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.LastModifiedByName, opt => opt.MapFrom(src => src.LastModifiedBy != null ? $"{src.LastModifiedBy.FirstName} {src.LastModifiedBy.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.IsRecentlyUpdated, opt => opt.MapFrom(src => src.IsRecentlyUpdated));
            CreateMap<CreateHelpArticleDto, HelpArticle>();
            CreateMap<UpdateHelpArticleDto, HelpArticle>();
            CreateMap<HelpArticle, HelpArticleListDto>()
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName));
        }
    }
}
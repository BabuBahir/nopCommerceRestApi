using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Rest.Models;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Api.Rest.Mappings;

public static class StoreProductOverviewMappingExtensions
{
    public static StoreProductOverviewDto ToStoreOverviewDto(this ProductOverviewModel model)
    {
        return new StoreProductOverviewDto
        {
            Id = model.Id,
            Name = model.Name,
            ShortDescription = model.ShortDescription,
            FullDescription = model.FullDescription,
            SeName = model.SeName,
            Sku = model.Sku,
            ProductType = model.ProductType.ToString(),
            MarkAsNew = model.MarkAsNew,
            ProductPrice = model.ProductPrice.ToStoreDto(),
            PictureModels = model.PictureModels.Select(picture => new StoreProductPictureDto
            {
                ImageUrl = picture.ImageUrl,
                ThumbImageUrl = picture.ThumbImageUrl,
                FullSizeImageUrl = picture.FullSizeImageUrl,
                Title = picture.Title,
                AlternateText = picture.AlternateText,
                CustomProperties = new Dictionary<string, string>(picture.CustomProperties)
            }).ToList(),
            ProductSpecificationModel = new StoreProductSpecificationDto
            {
                Groups = model.ProductSpecificationModel.Groups.Select(group => new StoreProductSpecificationGroupDto
                {
                    Name = group.Name,
                    Id = group.Id,
                    Attributes = group.Attributes.Select(attribute => new StoreProductSpecificationAttributeDto
                    {
                        Name = attribute.Name,
                        Id = attribute.Id,
                        Values = attribute.Values.Select(value => new StoreProductSpecificationValueDto
                        {
                            AttributeTypeId = value.AttributeTypeId,
                            ValueRaw = value.ValueRaw,
                            ColorSquaresRgb = value.ColorSquaresRgb,
                            CustomProperties = new Dictionary<string, string>(value.CustomProperties)
                        }).ToList(),
                        CustomProperties = new Dictionary<string, string>(attribute.CustomProperties)
                    }).ToList(),
                    CustomProperties = new Dictionary<string, string>(group.CustomProperties)
                }).ToList(),
                CustomProperties = new Dictionary<string, string>(model.ProductSpecificationModel.CustomProperties)
            },
            ReviewOverviewModel = new StoreProductReviewOverviewDto
            {
                ProductId = model.ReviewOverviewModel.ProductId,
                RatingSum = model.ReviewOverviewModel.RatingSum,
                TotalReviews = model.ReviewOverviewModel.TotalReviews,
                AllowCustomerReviews = model.ReviewOverviewModel.AllowCustomerReviews,
                CanAddNewReview = model.ReviewOverviewModel.CanAddNewReview,
                CustomProperties = new Dictionary<string, string>(model.ReviewOverviewModel.CustomProperties)
            },
            ProductToWishlist = new StoreProductWishlistDto
            {
                ProductId = model.ProductToWishlist.ProductId,
                CustomWishlistItems = model.ProductToWishlist.CustomWishlistItems.Select(item => new StoreProductWishlistItemDto
                {
                    Name = item.Name,
                    Id = item.Id,
                    CustomProperties = new Dictionary<string, string>(item.CustomProperties)
                }).ToList()
            },
            CustomProperties = new Dictionary<string, string>(model.CustomProperties)
        };
    }

    public static StoreProductPriceDto ToStoreDto(this ProductPriceModel model)
    {
        return new StoreProductPriceDto
        {
            OldPrice = model.OldPrice,
            OldPriceValue = model.OldPriceValue,
            Price = model.Price,
            PriceValue = model.PriceValue,
            BasePricePAngV = model.BasePricePAngV,
            BasePricePAngVValue = model.BasePricePAngVValue,
            DisableBuyButton = model.DisableBuyButton,
            DisableWishlistButton = model.DisableWishlistButton,
            DisableAddToCompareListButton = model.DisableAddToCompareListButton,
            AvailableForPreOrder = model.AvailableForPreOrder,
            PreOrderAvailabilityStartDateTimeUtc = model.PreOrderAvailabilityStartDateTimeUtc,
            IsRental = model.IsRental,
            ForceRedirectionAfterAddingToCart = model.ForceRedirectionAfterAddingToCart,
            DisplayTaxShippingInfo = model.DisplayTaxShippingInfo,
            CurrencyCode = model.CurrencyCode,
            PriceWithDiscount = model.PriceWithDiscount,
            PriceWithDiscountValue = model.PriceWithDiscountValue,
            CustomerEntersPrice = model.CustomerEntersPrice,
            CallForPrice = model.CallForPrice,
            ProductId = model.ProductId,
            HidePrices = model.HidePrices,
            RentalPrice = model.RentalPrice,
            RentalPriceValue = model.RentalPriceValue,
            CustomProperties = new Dictionary<string, string>(model.CustomProperties)
        };
    }

    public static async Task<List<StoreProductOverviewDto>> PrepareStoreOverviewDtosAsync(
        this IProductModelFactory productModelFactory,
        IEnumerable<Product> products)
    {
        var models = await productModelFactory.PrepareProductOverviewModelsAsync(products);

        return models.Select(model => model.ToStoreOverviewDto()).ToList();
    }

    public static async Task<PagedResult<StoreProductOverviewDto>> ToPagedStoreOverviewDtoAsync(
        this IPagedList<Product> products,
        IProductModelFactory productModelFactory)
    {
        return new PagedResult<StoreProductOverviewDto>
        {
            Items = await productModelFactory.PrepareStoreOverviewDtosAsync(products),
            PageIndex = products.PageIndex,
            PageSize = products.PageSize,
            TotalCount = products.TotalCount,
            TotalPages = products.TotalPages
        };
    }
}

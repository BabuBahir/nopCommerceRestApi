using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Rest.Models;

public class StoreProductOverviewDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("short_description")]
    public string ShortDescription { get; set; } = string.Empty;

    [JsonProperty("full_description")]
    public string FullDescription { get; set; } = string.Empty;

    [JsonProperty("se_name")]
    public string SeName { get; set; } = string.Empty;

    [JsonProperty("sku")]
    public string Sku { get; set; } = string.Empty;

    [JsonProperty("product_type")]
    public string ProductType { get; set; } = string.Empty;

    [JsonProperty("mark_as_new")]
    public bool MarkAsNew { get; set; }

    [JsonProperty("product_price")]
    public StoreProductPriceDto ProductPrice { get; set; } = new();

    [JsonProperty("picture_models")]
    public IList<StoreProductPictureDto> PictureModels { get; set; } = new List<StoreProductPictureDto>();

    [JsonProperty("product_specification_model")]
    public StoreProductSpecificationDto ProductSpecificationModel { get; set; } = new();

    [JsonProperty("review_overview_model")]
    public StoreProductReviewOverviewDto ReviewOverviewModel { get; set; } = new();

    [JsonProperty("product_to_wishlist")]
    public StoreProductWishlistDto ProductToWishlist { get; set; } = new();

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductPriceDto
{
    [JsonProperty("old_price")]
    public string OldPrice { get; set; } = string.Empty;

    [JsonProperty("old_price_value")]
    public decimal? OldPriceValue { get; set; }

    [JsonProperty("price")]
    public string Price { get; set; } = string.Empty;

    [JsonProperty("price_value")]
    public decimal? PriceValue { get; set; }

    [JsonProperty("base_price_p_ang_v")]
    public string BasePricePAngV { get; set; } = string.Empty;

    [JsonProperty("base_price_p_ang_v_value")]
    public decimal? BasePricePAngVValue { get; set; }

    [JsonProperty("disable_buy_button")]
    public bool DisableBuyButton { get; set; }

    [JsonProperty("disable_wishlist_button")]
    public bool DisableWishlistButton { get; set; }

    [JsonProperty("disable_add_to_compare_list_button")]
    public bool DisableAddToCompareListButton { get; set; }

    [JsonProperty("available_for_pre_order")]
    public bool AvailableForPreOrder { get; set; }

    [JsonProperty("pre_order_availability_start_date_time_utc")]
    public System.DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

    [JsonProperty("is_rental")]
    public bool IsRental { get; set; }

    [JsonProperty("force_redirection_after_adding_to_cart")]
    public bool ForceRedirectionAfterAddingToCart { get; set; }

    [JsonProperty("display_tax_shipping_info")]
    public bool DisplayTaxShippingInfo { get; set; }

    [JsonProperty("currency_code")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonProperty("price_with_discount")]
    public string PriceWithDiscount { get; set; } = string.Empty;

    [JsonProperty("price_with_discount_value")]
    public decimal? PriceWithDiscountValue { get; set; }

    [JsonProperty("customer_enters_price")]
    public bool CustomerEntersPrice { get; set; }

    [JsonProperty("call_for_price")]
    public bool CallForPrice { get; set; }

    [JsonProperty("product_id")]
    public int ProductId { get; set; }

    [JsonProperty("hide_prices")]
    public bool HidePrices { get; set; }

    [JsonProperty("rental_price")]
    public string RentalPrice { get; set; } = string.Empty;

    [JsonProperty("rental_price_value")]
    public decimal? RentalPriceValue { get; set; }

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductPictureDto
{
    [JsonProperty("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonProperty("thumb_image_url")]
    public string ThumbImageUrl { get; set; } = string.Empty;

    [JsonProperty("full_size_image_url")]
    public string FullSizeImageUrl { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("alternate_text")]
    public string AlternateText { get; set; } = string.Empty;

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductSpecificationDto
{
    [JsonProperty("groups")]
    public IList<StoreProductSpecificationGroupDto> Groups { get; set; } = new List<StoreProductSpecificationGroupDto>();

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductSpecificationGroupDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("attributes")]
    public IList<StoreProductSpecificationAttributeDto> Attributes { get; set; } = new List<StoreProductSpecificationAttributeDto>();

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductSpecificationAttributeDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("values")]
    public IList<StoreProductSpecificationValueDto> Values { get; set; } = new List<StoreProductSpecificationValueDto>();

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductSpecificationValueDto
{
    [JsonProperty("attribute_type_id")]
    public int AttributeTypeId { get; set; }

    [JsonProperty("value_raw")]
    public string ValueRaw { get; set; } = string.Empty;

    [JsonProperty("color_squares_rgb")]
    public string ColorSquaresRgb { get; set; } = string.Empty;

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductReviewOverviewDto
{
    [JsonProperty("product_id")]
    public int ProductId { get; set; }

    [JsonProperty("rating_sum")]
    public int RatingSum { get; set; }

    [JsonProperty("total_reviews")]
    public int TotalReviews { get; set; }

    [JsonProperty("allow_customer_reviews")]
    public bool AllowCustomerReviews { get; set; }

    [JsonProperty("can_add_new_review")]
    public bool CanAddNewReview { get; set; }

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

public class StoreProductWishlistDto
{
    [JsonProperty("product_id")]
    public int ProductId { get; set; }

    [JsonProperty("custom_wishlist_items")]
    public IList<StoreProductWishlistItemDto> CustomWishlistItems { get; set; } = new List<StoreProductWishlistItemDto>();
}

public class StoreProductWishlistItemDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("custom_properties")]
    public IDictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
}

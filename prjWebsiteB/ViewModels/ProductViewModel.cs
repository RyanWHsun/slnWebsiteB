using prjWebsiteB.Models;
using System.ComponentModel.DataAnnotations;

namespace prjWebsiteB.ViewModels
{
    public class ProductViewModel
    {
        public int FProductId { get; set; }
        public int? FUserId { get; set; }
        [Required(ErrorMessage = "Please select a product category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a product category.")]
        public int? FProductCategoryId { get; set; }
        public string FProductName { get; set; }
        public string FProductDescription { get; set; }
        [Required(ErrorMessage = "金額必填喔!")]
        public decimal? FProductPrice { get; set; }
        public bool? FIsOnSale { get; set; }
        public DateTime? FProductDateAdd { get; set; }
        public DateTime? FProductUpdated { get; set; }
        [Required(ErrorMessage = "數量必填喔!")]
        public int? FStock { get; set; }
        public string? FCategoryName { get; set; }  //導覽屬性
        public int FProductImageId { get; set; }
        public List<IFormFile>? FImage { get; set; } // 用於存取圖片
    }

}

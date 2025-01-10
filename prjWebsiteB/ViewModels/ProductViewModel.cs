using prjWebsiteB.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace prjWebsiteB.ViewModels
{
    public class ProductViewModel
    {
        [DisplayName("商品ID")]
        public int FProductId { get; set; }
        [DisplayName("賣家ID")]
        public int? FUserId { get; set; }
        [Required(ErrorMessage = "商品類別必須選哦")]
        [Range(1, int.MaxValue, ErrorMessage = "商品類別必須選哦")]
        [DisplayName("類別名稱")]
        public int? FProductCategoryId { get; set; }
        [DisplayName("商品名稱")]
        [StringLength(50, ErrorMessage = "名稱請在50字內")]
        [Required(ErrorMessage = "商品名稱必須寫哦")]
        public string FProductName { get; set; }
        [DisplayName("商品描述")]
        [StringLength(200, ErrorMessage = "描述請在200字內")]
        public string? FProductDescription { get; set; }
        [Required(ErrorMessage = "金額必填喔!")]
        [DisplayFormat(DataFormatString = "NT${0:N0}")]
        [Range(1, 99999, ErrorMessage = "請填入合理金額哦")]
        [DisplayName("商品金額")]
        public decimal? FProductPrice { get; set; }
        [DisplayName("狀態")]
        public bool? FIsOnSale { get; set; }
        [DisplayName("新增日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? FProductDateAdd { get; set; }
        [DisplayName("修改日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? FProductUpdated { get; set; }
        [Required(ErrorMessage = "數量必填喔!")]
        [Range(1, 99999, ErrorMessage = "請填入合理數量喔")]
        [DisplayName("庫存")]
        public int? FStock { get; set; }
        [DisplayName("類別名稱")]
        public string? FCategoryName { get; set; }  //導覽屬性
        [DisplayName("商品照片")]
        public List<IFormFile>? FImage { get; set; } // 用於存取圖片
    }

}

using prjWebsiteB.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace prjWebsiteB.AttractionViewModels {
    public class TAttraction {
        public int FAttractionId { get; set; }

        [Display(Name = "景點名稱")]
        [Required(ErrorMessage = "景點名稱欄位未填寫")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "景點名稱長度不合法")]
        public string FAttractionName { get; set; }

        [DisplayName("景點介紹")]
        [Required(ErrorMessage = "景點介紹欄位未填寫")]
        public string FDescription { get; set; }

        [DisplayName("地址")]
        [Required(ErrorMessage = "地址欄位未填寫")]
        public string FAddress { get; set; }

        [DisplayName("電話")]
        public string? FPhoneNumber { get; set; }

        [DisplayName("開放時間")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString ="{0:HH:mm}")]
        public TimeOnly? FOpeningTime { get; set; }

        [DisplayName("關閉時間")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public TimeOnly? FClosingTime { get; set; }

        [DisplayName("網站")]
        [Required(ErrorMessage = "網站欄位未填寫")]
        [Url(ErrorMessage = "網站格式不正確")]
        public string FWebsiteUrl { get; set; }

        [DisplayName("經度")]
        public string? FLongitude { get; set; }

        [DisplayName("緯度")]
        public string? FLatitude { get; set; }

        [DisplayName("地區")]
        public string? FRegion { get; set; }

        [DisplayName("分類")]
        [Required(ErrorMessage = "分類欄位未填寫")]
        public int? FCategoryId { get; set; }

        [DisplayName("建立日期")]
        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString ="{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? FCreatedDate { get; set; }

        [DisplayName("更新日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? FUpdatedDate { get; set; }

        [DisplayName("狀態")]
        public string? FStatus { get; set; }

        [DisplayName("交通資訊")]
        public string? FTrafficInformation { get; set; }

        public virtual TAttractionCategory FCategory { get; set; }

        public virtual ICollection<TAttractionComment> TAttractionComments { get; set; } = new List<TAttractionComment>();

        public virtual ICollection<TAttractionImage> TAttractionImages { get; set; } = new List<TAttractionImage>();

        public virtual ICollection<TAttractionTicket> TAttractionTickets { get; set; } = new List<TAttractionTicket>();
    }
}

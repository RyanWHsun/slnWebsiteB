using System.ComponentModel.DataAnnotations;

namespace prjWebsiteB.ViewModels
{
    public class TEventCreateViewModel : IValidatableObject
    {
        public string? FEventName { get; set; }
        public string? FEventDescription { get; set; }

        [Required(ErrorMessage = "開始日期為必填項")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "日期格式必須為 yyyy-MM-dd")]
        public string? FEventStartDate { get; set; }

        [Required(ErrorMessage = "結束日期為必填項")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "日期格式必須為 yyyy-MM-dd")]
        public string? FEventEndDate { get; set; }

        public string? FEventLocation { get; set; }
        public decimal? FEventActivityfee { get; set; }
        public int? FEventCategoryId { get; set; } // 活動類型 ID
        public int? FUserId { get; set; } // 建立者 ID
        public bool FEventIsActive { get; set; }
        public DateTime? FEventCreatedDate { get; set; }

        [Required(ErrorMessage = "人數上限為必填項")]
        [Range(1, int.MaxValue, ErrorMessage = "人數上限必須大於 0")]
        public int? FEventMaxParticipants { get; set; }

        public int FEventId { get; internal set; }
        public List<IFormFile>? UploadedFiles { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // 檢查開始日期和結束日期格式是否正確
            if (!DateTime.TryParse(FEventStartDate, out var startDate))
            {
                results.Add(new ValidationResult("無效的開始日期格式", new[] { nameof(FEventStartDate) }));
            }

            if (!DateTime.TryParse(FEventEndDate, out var endDate))
            {
                results.Add(new ValidationResult("無效的結束日期格式", new[] { nameof(FEventEndDate) }));
            }

            // 如果格式正確，檢查開始日期是否早於結束日期
            if (startDate > endDate)
            {
                results.Add(new ValidationResult("開始日期不能晚於結束日期", new[] { nameof(FEventStartDate), nameof(FEventEndDate) }));
            }

            // 其他自定義驗證邏輯可以在這裡添加，例如活動名稱必填但不超過 30 字
            if (!string.IsNullOrEmpty(FEventName) && FEventName.Length > 30)
            {
                results.Add(new ValidationResult("活動名稱不能超過 30 個字元", new[] { nameof(FEventName) }));
            }

            return results;
        }
    }
}
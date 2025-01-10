using System.ComponentModel.DataAnnotations;

namespace prjWebsiteB.Models
{
    internal class TPostMetadata
    {
        [Display(Name = "文章編號")]
        public int FPostId { get; set; }
        [Display(Name = "作者編號")]
        public int? FUserId { get; set; }
        [Display(Name = "文章標題")]
        public string FTitle { get; set; }
        [Display(Name = "文章內容")]
        public string FContent { get; set; }
        [Display(Name = "創建時間")]
        public DateTime? FCreatedAt { get; set; }
        [Display(Name = "最後修改時間")]
        public DateTime? FUpdatedAt { get; set; }
        [Display(Name = "文章狀態")]
        public bool? FIsPublic { get; set; }
        [Display(Name = "類別編號")]
        public int? FCategoryId { get; set; }
    }
}
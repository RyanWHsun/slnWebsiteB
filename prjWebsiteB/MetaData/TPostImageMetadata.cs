using System.ComponentModel.DataAnnotations;

namespace prjWebsiteB.Models
{
    internal class TPostImageMetadata
    {
        [Display(Name ="文章圖片")]
        public byte[] FImage { get; set; }
    }
}
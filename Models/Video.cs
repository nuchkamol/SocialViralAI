namespace SocialViralAI.Models
{
    public class Video
    {
        public string Title { get; set; }
        public long Views { get; set; }
        public long Likes { get; set; }
        public long Comments { get; set; }

        public string VideoId { get; set; } // 🔥 เพิ่มอันนี้
        public int ViralScore { get; set; }
        public bool IsShort { get; set; }
        public bool IsTop { get; set; }
    }
}

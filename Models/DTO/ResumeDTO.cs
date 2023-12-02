namespace IngBackend.Models.DTO
{
    public class ResumeDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public List<ResumeAreaDTO<TextLayoutDTO>>? TextLayouts { get; set; }
        public List<ResumeAreaDTO<ImageTextLayoutDTO>>? ImageTextLayouts { get; set; }
    }

    public class ResumePostDTO
    {
        public Guid? Id { get; set; }
        public required string Title { get; set; }
        public List<ResumeAreaDTO<TextLayoutDTO>>? TextLayouts { get; set; }
        public List<ResumeAreaDTO<ImageTextLayoutDTO>>? ImageTextLayouts { get; set; }
    }

    public class ResumeAreaDTO<TLayout>
    {
        public required TLayout Layout { get; set; }
        public required int Sequence { get; set; }
    }

    public class ImageDTO
    {
        public Guid? Id { get; set; }
        public required byte[] Content { get; set; }
    }

    public class LayoutDTO
    {
        public string Type { get; set; } = "custom";
        public string Title { get; set; } = "";

    }

    public class TextLayoutDTO : LayoutDTO
    {
        public string Name { get; set; } = "文字";
        public string Content { get; set; } = "";
    }
    public class ImageTextLayoutDTO : LayoutDTO
    {
        public string Name { get; set; } = "圖片文字";
        public string Content { get; set; } = "";
        public ImageDTO? Image { get; set; }
    }
}

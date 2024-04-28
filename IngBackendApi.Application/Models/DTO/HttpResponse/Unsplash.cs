namespace IngBackendApi.Models.DTO.HttpResponse;

public class UnsplashSearchResponse
{
    public class ImageLinksArea
    {
        public string Download { get; set; }
    }

    public class ImageArea
    {
        public string Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Color { get; set; }
        public string? Description { get; set; }
        public ImageSource Urls { get; set; }
        public ImageLinksArea Links { get; set; }
    }

    public int Total { get; set; }
    public IEnumerable<ImageArea> Results { get; set; }
}

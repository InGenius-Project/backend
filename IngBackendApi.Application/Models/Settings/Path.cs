namespace IngBackendApi.Models.Settings;

public class PathSetting : Setting
{
    public class ImageArea
    {
        public string Avatar { get; set; }
        public string Area { get; set; }
    }

    public new string Name { get; set; } = "Image Setting";
    public ImageArea Image { get; set; }
}

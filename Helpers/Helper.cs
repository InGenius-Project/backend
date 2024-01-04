namespace IngBackend.Helpers;

public static class Helper
{
    public static bool IsInDocker()
    {
        // 檢查環境變數是否存在
        if (Environment.GetEnvironmentVariable("DOCKER_HOST") != null)
        {
            // 容器中存在 DOCKER_HOST 環境變數
            return true;
        }
        else
        {
            // 容器中不存在 DOCKER_HOST 環境變數
            return false;
        }
    } 

    public static string GetSAPassword()
    {
        var pw = Environment.GetEnvironmentVariable("SA_PASSWORD");
        if (pw == null)
        {
            return "";
        }

        return pw;
    }
}

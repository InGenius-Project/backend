using IngBackendApi.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IngBackendApi.Services.Middlewares;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class ApiResponseMiddleware : IMiddleware
{
    private readonly ILogger _logger;

    public ApiResponseMiddleware(ILogger<ApiResponseMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBody = context.Response.Body;

        try
        {
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, ex.Message);

                await HandleException(context, memStream, ex);
            }

            // 找出 content type
            string contentType = context.Response.ContentType ?? "";
            bool isFileResponse =
                !contentType.StartsWith("application/problem+json")
                    && !contentType.StartsWith("application/json")
                    && contentType.StartsWith("application/")
                || contentType.StartsWith("image/")
                || contentType.StartsWith("audio/")
                || contentType.StartsWith("video/");

            // 如果不是 檔案 才進行 parse
            if (!isFileResponse)
            {
                // 在回應發送之後進行統一格式化處理
                await FormatResponseAsync(context, memStream);
            }

            memStream.Position = 0;

            // 將新的內容寫入原始的回應主體
            await memStream.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    public async Task FormatResponseAsync(HttpContext context, MemoryStream memoryStream)
    {
        // 將記憶體串流設定為回應的主體
        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var parsedBody = ParseApiResponse(responseBody, context.Response.StatusCode);

        context.Response.ContentType = "application/json";

        // 清空原本的資料
        memoryStream.SetLength(0);

        // 寫入內容
        await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(parsedBody));
    }

    private static string ParseApiResponse(string originalBody, int statusCode)
    {
        JToken? jsonToken;
        string? message = "";
        string? exception = "";
        try
        {
            jsonToken = JToken.Parse(originalBody);
            message = jsonToken["Message"].ToString();
            exception = jsonToken["ExceptionType"].ToString();
            Console.WriteLine("意外: {0}", exception);
        }
        catch (JsonReaderException)
        {
            jsonToken = null;
        }

        var isSuccess = statusCode >= 200 && statusCode < 300;

        // 不是 json 物件
        if (jsonToken == null)
        {
            if (isSuccess)
            {
                var responseRawObject = new
                {
                    Success = isSuccess,
                    StatusCode = statusCode,
                    Message = message ?? GetDefaultMessage(statusCode),
                    Data = originalBody,
                };

                return JsonConvert.SerializeObject(responseRawObject);
            }
            else
            {
                var responseRawObject = new
                {
                    Success = isSuccess,
                    StatusCode = statusCode,
                    Message = message ?? GetDefaultMessage(statusCode),
                    Exception = originalBody,
                };

                return JsonConvert.SerializeObject(responseRawObject);
            }
        }

        // 是 json 物件
        if (jsonToken.Type == JTokenType.Object)
        {
            // 轉換成 jsonObject
            JObject jsonObject = (JObject)jsonToken;
            jsonObject["Exception"] = jsonObject["InnerException"];
            // Bad request
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                jsonToken["Exception"] = jsonObject["errors"];
            }

            // 寫回 JsonToken
            jsonToken = jsonObject;
        }

        if (isSuccess)
        {
            var responseObject = new
            {
                Success = isSuccess,
                StatusCode = statusCode,
                Message = message ?? GetDefaultMessage(statusCode),
                Data = jsonToken
            };

            return JsonConvert.SerializeObject(responseObject);
        }
        else
        {
            var responseObject = new
            {
                Success = isSuccess,
                StatusCode = statusCode,
                Message = message ?? GetDefaultMessage(statusCode),
                Exception = jsonToken["ExceptionType"]
            };
            return JsonConvert.SerializeObject(responseObject);
        }
    }

    private static string GetDefaultMessage(int statusCode)
    {
        // 根據狀態碼返回預設訊息
        return statusCode switch
        {
            200 => "OK",
            201 => "資源已創建",
            202 => "請求已接受",
            400 => "請求無效",
            401 => "驗證錯誤",
            403 => "拒絕存取",
            404 => "已被刪除、移動或從未存在",
            _ => "發生未知的錯誤",
        };
    }

    private static ValueTask HandleException(
        HttpContext context,
        MemoryStream memoryStream,
        Exception exception
    )
    {
        string? message = null;
        HttpStatusCode? code = null;

        if (exception is UnauthorizedException)
        {
            message = "驗證錯誤";
            code = HttpStatusCode.Unauthorized; // 401
        }
        else if (exception is NotFoundException)
        {
            message = "已被刪除、移動或從未存在";
            code = HttpStatusCode.NotFound; // 404
        }
        else if (exception is UserNotFoundException)
        {
            message = "使用者不存在";
            code = HttpStatusCode.NotFound; // 404
        }
        else if (exception is ForbiddenException)
        {
            message = "拒絕存取";
            code = HttpStatusCode.Forbidden;
        }
        else if (exception is BadRequestException || exception is BadHttpRequestException)
        {
            message = "請求無效";
            code = HttpStatusCode.BadRequest; // 400
        }
        else if (exception is SecurityTokenValidationException)
        {
            message = "JWT token 驗證失敗";
        }

        if (code == null)
        {
            code = HttpStatusCode.InternalServerError; // 預設狀態碼
        }

        var exceptionMessage = exception.Message.ToString();
        if (exceptionMessage != null)
        {
            message = exceptionMessage;
        }

        if (message == null)
        {
            message = "伺服器發生內部錯誤";
        }

        var jsonObject = new { Message = message, ExceptionType = exception.GetType().Name };
        var result = JsonConvert.SerializeObject(jsonObject);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return memoryStream.WriteAsync(Encoding.UTF8.GetBytes(result));
    }
}

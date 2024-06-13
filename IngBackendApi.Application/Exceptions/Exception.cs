namespace IngBackendApi.Exceptions;

using AutoWrapper.Wrappers;
using static Microsoft.AspNetCore.Http.StatusCodes;

public class JsonParseException : Exception
{
    public JsonParseException()
        : base("Json Parse Error") =>
        throw new ApiException("Json Parse Error", Status500InternalServerError);

    public JsonParseException(string message)
        : base(message) => throw new ApiException(message, Status500InternalServerError);
}

public class SystemInitException : Exception
{
    public SystemInitException()
        : base("系統初始化錯誤") => throw new ApiException("系統初始化錯誤", Status500InternalServerError);

    public SystemInitException(string message)
        : base(message) => throw new ApiException(message, Status500InternalServerError);

    public SystemInitException(Exception ex) =>
        throw new ApiException(ex, Status500InternalServerError);
}

public class SystemInternalException : ApiException
{
    public SystemInternalException()
        : base("系統內部錯誤") => throw new ApiException("系統內部錯誤", Status500InternalServerError);

    public SystemInternalException(string message)
        : base($"系統內部錯誤: {message}") =>
        throw new ApiException("系統內部錯誤", Status500InternalServerError);
}

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
        : base() { }

    public EntityNotFoundException(Exception ex)
        : base(ex.Message) { }

    public EntityNotFoundException(string message)
        : base(message) { }

    public EntityNotFoundException(string message, Exception exception)
        : base(message, exception) { }
}

#region 400 Errors

public class BadRequestException : ApplicationException
{
    public BadRequestException()
        : base() => throw new ApiException("請求錯誤", Status400BadRequest);

    public BadRequestException(string message)
        : base(message) => throw new ApiException(message, Status400BadRequest);

    public BadRequestException(Exception ex) => throw new ApiException(ex, Status400BadRequest);
}

#endregion

#region 401 Errors

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException()
        : base("未經授權") => throw new ApiException("未經授權", Status401Unauthorized);

    public UnauthorizedException(string message) =>
        throw new ApiException(message, Status401Unauthorized);

    public UnauthorizedException(Exception ex) => throw new ApiException(ex, Status401Unauthorized);
}

#endregion

#region 403 Errors

public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("拒絕存取") => throw new ApiException("拒絕存取", Status403Forbidden);

    public ForbiddenException(string message)
        : base(message) => throw new ApiException(message, Status403Forbidden);

    public ForbiddenException(Exception ex) => throw new ApiException(ex, Status403Forbidden);
}

#endregion

#region 404 Errors

public class NotFoundException : ApplicationException
{
    public NotFoundException()
        : base() => throw new ApiException("資料不存在", Status404NotFound);

    public NotFoundException(string message)
        : base(message) => throw new ApiException($"{message}不存在", Status404NotFound);
}

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException()
        : base("使用者") { }
}

public class TagNotFoundException(string tagId) : NotFoundException($"標籤: {tagId}") { }

public class AreaNotFoundException : NotFoundException
{
    public AreaNotFoundException()
        : base("區域") { }

    public AreaNotFoundException(string areaId)
        : base($"區域: {areaId}") { }
}

public class ResumeNotFoundException : NotFoundException
{
    public ResumeNotFoundException()
        : base("履歷") { }

    public ResumeNotFoundException(string resumeId)
        : base($"履歷: {resumeId}") { }
}

public class RecruitmentNotFoundException : NotFoundException
{
    public RecruitmentNotFoundException()
        : base("職缺") { }

    public RecruitmentNotFoundException(string recruitmentId)
        : base($"職缺: {recruitmentId}") { }
}

public class ChatGroupNotFoundException : NotFoundException
{
    public ChatGroupNotFoundException()
        : base("聊天群組") { }

    public ChatGroupNotFoundException(string groupId)
        : base($"聊天群組: {groupId}") { }
}

public class ChatGroupInvitationNotFoundException : NotFoundException
{
    public ChatGroupInvitationNotFoundException()
        : base("聊天群組邀請") { }

    public ChatGroupInvitationNotFoundException(string invitationId)
        : base($"聊天群組邀請: {invitationId}") { }
}

#endregion

#region System Internal Exception
public class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException()
        : base("System Configuration File Not Found.") =>
        throw new SystemInternalException($"System Configuration File Not Found.");

    public ConfigurationNotFoundException(string configPath)
        : base($"System Configuration File Not Found: {configPath}.") =>
        throw new SystemInternalException($"System Configuration File Not Found: {configPath}.");
}

public class ServerNetworkException : Exception
{
    public ServerNetworkException()
        : base("Server Network Connection Error.") =>
        throw new SystemInternalException("Server Network Connection Error.");

    public ServerNetworkException(string message)
        : base($"Server Network Connection Error: {message}.") =>
        throw new SystemInternalException($"Server Network Connection Error: {message}.");
}

#endregion

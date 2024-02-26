namespace IngBackend.Exceptions;
using AutoWrapper.Wrappers;
using static Microsoft.AspNetCore.Http.StatusCodes;

// 400
public class BadRequestException : ApplicationException
{
    public BadRequestException() : base() => throw new ApiException("請求錯誤", Status400BadRequest);

    public BadRequestException(string message) : base(message) => throw new ApiException(message, Status400BadRequest);

    public BadRequestException(Exception ex) => throw new ApiException(ex, Status400BadRequest);
}

// 401
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException() : base("未經授權") => throw new ApiException("未經授權", Status401Unauthorized);

    public UnauthorizedException(string message) => throw new ApiException(message, Status401Unauthorized);

    public UnauthorizedException(Exception ex) => throw new ApiException(ex, Status401Unauthorized);
}

// 403 
public class ForbiddenException : Exception
{
    public ForbiddenException() : base("拒絕存取") => throw new ApiException("拒絕存取", Status403Forbidden);

    public ForbiddenException(string message) : base(message) => throw new ApiException(message, Status403Forbidden);

    public ForbiddenException(Exception ex) => throw new ApiException(ex, Status403Forbidden);
}

// 404
public class NotFoundException : ApplicationException
{
    public NotFoundException() : base() => throw new ApiException("資料不存在", Status404NotFound);

    public NotFoundException(string message) : base(message) => throw new ApiException($"{message}不存在", Status404NotFound);
    public NotFoundException(string message, Exception exception) : base(message, exception) => throw new ApiException(exception, Status404NotFound);
}

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException() : base("使用者") { }
}

public class TagNotFoundException(string tagId) : NotFoundException($"標籤: {tagId}")
{
}

public class AreaNotFoundException : NotFoundException
{
    public AreaNotFoundException() : base("區域") { }
    public AreaNotFoundException(string areaId) : base($"區域: {areaId}") { }
}

public class TagTypeNotFoundExceptiuon() : NotFoundException("標籤類型") { }

public class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException() : base() { }
    public EntityNotFoundException(Exception ex) : base(ex.Message)
    { }
    public EntityNotFoundException(string message) : base(message) { }
    public EntityNotFoundException(string message, Exception exception) : base(message, exception)
    { }
}



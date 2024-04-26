namespace IngBackendApi.Enum;

public enum UserRole
{
    Intern,
    Company,
    Admin,
    InternalUser
}

public enum LayoutType
{
    Text,
    ImageText,
    List,
    KeyValueList
}

public enum AreaPostType
{
    User,
    Resume,
    Recruitment
}

public enum ChatReceiveMethod
{
    Message,
    NewGroup,
    JoinGroup,
    BroadCast,
    LastMessage
}

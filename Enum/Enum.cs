using System.ComponentModel;
namespace lng_backend.Enum;

public enum UserRole
{
    [Description("系統管理員")]
    Admin,
    [Description("內部人員")]
    Internal,
    [Description("實習使用者")]
    Intern,
    [Description("企業使用者")]
    Company
}

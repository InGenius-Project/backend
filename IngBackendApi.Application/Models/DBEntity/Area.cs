namespace IngBackendApi.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using IngBackendApi.Enum;
using IngBackendApi.Interfaces.Models;
using IngBackendApi.Interfaces.Repository;

public class Area : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; } = true;
    public required string Title { get; set; }
    public LayoutType? LayoutType { get; set; } // custom area

    public Guid? ResumeId { get; set; }

    [JsonIgnore]
    public Resume? Resume { get; set; }

    public Guid? UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    public Guid OwnerId { get; set; }
    public User Owner { get; set; }

    public Guid? RecruitmentId { get; set; }

    [JsonIgnore]
    public Recruitment? Recruitment { get; set; }

    [JsonIgnore]
    [ForeignKey("AreaType")]
    public int? AreaTypeId { get; set; }
    public AreaType? AreaType { get; set; } // default area

    public Guid? TextLayoutId { get; set; }

    [JsonIgnore]
    public virtual TextLayout? TextLayout { get; set; }

    public Guid? ImageTextLayoutId { get; set; }

    [JsonIgnore]
    public virtual ImageTextLayout? ImageTextLayout { get; set; }

    public Guid? ListLayoutId { get; set; }

    [JsonIgnore]
    public virtual ListLayout? ListLayout { get; set; }

    public Guid? KeyValueListLayoutId { get; set; }

    [JsonIgnore]
    public virtual KeyValueListLayout? KeyValueListLayout { get; set; }

    public void ClearLayoutsExclude<TProperty>(Expression<Func<Area, TProperty>> propertyToRemain)
    {
        if (propertyToRemain.Body is not MemberExpression memberExpression)
        {
            return;
        }

        var memberInfo = memberExpression.Member;
        var propertyName = memberInfo.Name;

        switch (propertyName)
        {
            case nameof(TextLayout):
                ImageTextLayoutId = null;
                ImageTextLayout = null;
                ListLayoutId = null;
                ListLayout = null;
                KeyValueListLayoutId = null;
                KeyValueListLayout = null;
                break;
            case nameof(ImageTextLayout):
                TextLayoutId = null;
                TextLayout = null;
                ListLayoutId = null;
                ListLayout = null;
                KeyValueListLayoutId = null;
                KeyValueListLayout = null;
                break;
            case nameof(ListLayout):
                TextLayoutId = null;
                TextLayout = null;
                ImageTextLayoutId = null;
                ImageTextLayout = null;
                KeyValueListLayoutId = null;
                KeyValueListLayout = null;
                break;
            case nameof(KeyValueListLayout):
                TextLayoutId = null;
                TextLayout = null;
                ImageTextLayoutId = null;
                ImageTextLayout = null;
                ListLayoutId = null;
                ListLayout = null;
                break;
            default:
                break;
        }
    }

    public override string ToString()
    {
        var content = new StringBuilder();
        if (ListLayout != null)
        {
            content.Append(ListLayout.ToString());
        }
        else if (ImageTextLayout != null)
        {
            content.Append(ImageTextLayout.ToString());
        }
        else if (KeyValueListLayout != null)
        {
            content.Append(KeyValueListLayout.ToString());
        }
        else if (TextLayout != null)
        {
            content.Append(TextLayout.ToString());
        }
        else
        {
            content.Append("");
        }
        return $"{Title}: {content}";
    }
}

public class AreaType : BaseEntity, IEntity<int>
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; } // unique
    public required string Description { get; set; }
    public required List<UserRole> UserRole { get; set; }
    public required LayoutType LayoutType { get; set; }

    [JsonIgnore]
    public List<Area>? Areas { get; set; }

    [JsonIgnore]
    public List<TagType>? ListTagTypes { get; set; }
}

public class Image : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public string? Uri { get; set; }
    public string? AltContent { get; set; }
    public required string Filepath { get; set; }
    public required string ContentType { get; set; }
}

#region Area Layout
public class TextLayout : BaseEntity, IEntity<Guid>, IAreaLayout
{
    [Key]
    public Guid Id { get; set; }
    public string Content { get; set; } = "";

    [JsonIgnore]
    [Required]
    public Area Area { get; set; }

    [ForeignKey("Area")]
    [Required]
    public Guid? AreaId;

    public override string ToString() => Content;
}

public class ImageTextLayout : BaseEntity, IEntity<Guid>, IAreaLayout
{
    [Key]
    public Guid Id { get; set; }
    public string? TextContent { get; set; }
    public Image? Image { get; set; }

    [Required]
    [JsonIgnore]
    public required Area Area { get; set; }

    [ForeignKey("Area")]
    [Required]
    public Guid? AreaId;

    public override string ToString() => $"Image Description: {TextContent}";
}

public class ListLayout : BaseEntity, IEntity<Guid>, IAreaLayout
{
    [Key]
    public Guid Id { get; set; }

    public List<Tag>? Items { get; set; }

    [Required]
    public Guid? AreaId;

    [JsonIgnore]
    [Required]
    public Area Area { get; set; }

    public override string ToString()
    {
        var itemsString = string.Join(", ", Items?.Select(x => x.Name) ?? []);
        return itemsString;
    }
}

public class KeyValueListLayout : BaseEntity, IEntity<Guid>, IAreaLayout
{
    [Key]
    public Guid Id { get; set; }
    public List<KeyValueItem>? Items { get; set; }

    [JsonIgnore]
    [Required]
    public Area Area { get; set; }

    [ForeignKey("Area")]
    [Required]
    public Guid? AreaId;

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        Items?.ForEach(x =>
        {
            var keysString = string.Join(", ", x.Key?.Select(x => x.Name) ?? []);
            stringBuilder.AppendLine(
                CultureInfo.GetCultureInfo("zh-TW"),
                $"{x.Value}: {keysString}"
            );
        });

        return stringBuilder.ToString();
    }
}

public class KeyValueItem : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public List<Tag>? Key { get; set; }
    public string Value { get; set; } = "";

    public Guid? keyValueListLayoutId { get; set; }
    public KeyValueListLayout KeyValueListLayout { get; set; }
}
#endregion

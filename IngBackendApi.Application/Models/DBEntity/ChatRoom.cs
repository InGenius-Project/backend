namespace IngBackendApi.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngBackendApi.Interfaces.Repository;

public class ChatRoom : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "ChatRoomName 不能為空")]
    public string ChatRoomName { get; set; }
    public ICollection<User> Users { get; set; } = [];
    public ICollection<ChatMessage> Messages { get; set; } = [];
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    public bool Private { get; set; } = true;
}

public class ChatMessage : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "訊息不能為空")]
    public required string Message { get; set; }
    public Guid ChatRoomId { get; set; }

    [JsonIgnore]
    public ChatRoom ChatRoom { get; set; }
    public Guid SenderId { get; set; }
    public User Sender { get; set; }
}

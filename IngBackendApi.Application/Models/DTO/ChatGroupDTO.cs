namespace IngBackendApi.Models.DTO;

public class ChatGroupInfoDTO
{
    public Guid Id { get; set; }
    public required string GroupName { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreateTime { get; set; }
    public IEnumerable<OwnerUserDTO> Users { get; set; } = [];
    public OwnerUserDTO Owner { get; set; }
}

public class ChatMessageDTO
{
    public Guid Id { get; set; }

    public required string Message { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public OwnerUserDTO Sender { get; set; }
    public DateTime SendTime { get; set; }
}

public class ChatGroupDTO : ChatGroupInfoDTO
{
    public ICollection<ChatMessageDTO> ChatMessages { get; set; } = [];
}

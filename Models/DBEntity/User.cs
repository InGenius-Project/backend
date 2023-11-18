using IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace IngBackend.Models.DBEntity
{
    [Index(nameof(Email), IsUnique = true)]
    public class User : BaseEntity, IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(512)")]
        public string Email { get; set; }

        [MaxLength(124)]
        public string Username { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        public List<Resume>? Resumes { get; set; }
    }
}

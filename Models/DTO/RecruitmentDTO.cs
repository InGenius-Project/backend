using IngBackend.Enum;
using IngBackend.Models.DBEntity;

namespace IngBackend.Models.DTO
{
    public class RecruitmentDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public List<AreaDTO> Areas { get; set; }
        public IEnumerable<ResumeDTO> Resumes { get; set; }
        // public UserInfoDTO Publisher { get; set; }
        // public Guid PublisherId { get; set; }
    }

    public class RecruitmentPostDTO
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public List<AreaDTO> Areas { get; set; }
    }



}


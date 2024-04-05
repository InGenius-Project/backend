namespace IngBackendApi.Application.Interfaces.Service;

using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IResumeService : IService<Resume, ResumeDTO, Guid> { }

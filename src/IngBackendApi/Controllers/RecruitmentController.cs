using AutoMapper;
using IngBackendApi.Services.UserService;
using IngBackendApi.Services.RecruitmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using IngBackendApi.Exceptions;
using IngBackendApi.Services.AreaService;
using AutoWrapper.Wrappers;

namespace IngBackendApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RecruitmentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly RecruitmentService _recruitmentService;
        private readonly AreaService _areaService;

        public RecruitmentController(IMapper mapper, AreaService areaService, UserService userService, RecruitmentService recruitmentService)
        {
            _mapper = mapper;
            _areaService = areaService;
            _userService = userService;
            _recruitmentService = recruitmentService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<RecruitmentDTO>), 200)]
        public async Task<List<RecruitmentDTO>> GetRecruitmentsByUser()
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
            await _userService.CheckAndGetUserAsync(userId, Enum.UserRole.Company);

            var recruitments = _recruitmentService.GetUserRecruitements(userId);

            var recruitmentsDTO = _mapper.Map<List<RecruitmentDTO>>(recruitments);
            return recruitmentsDTO;
        }


        [HttpGet("{recruitmentId}")]
        [ProducesResponseType(typeof(RecruitmentDTO), 200)]
        public async Task<RecruitmentDTO?> GetRecruitmentById(Guid recruitmentId)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

            await _userService.CheckAndGetUserAsync(userId);
            var recruitment = await _recruitmentService.GetByIdAsync(recruitmentId);
            return recruitment;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseDTO<RecruitmentDTO>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> PostRecruitment([FromBody] RecruitmentPostDTO req)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
            var user = await _userService.CheckAndGetUserAsync(userId);

            var recruitment = await _recruitmentService.GetByIdAsync(req.Id ?? Guid.Empty);

            // Add new recruitment
            if (recruitment == null)
            {
                var newRecruitment = _mapper.Map<RecruitmentDTO>(req);
                newRecruitment.Publisher = user;
                await _recruitmentService.AddAsync(newRecruitment);
                return new ApiResponse("Post Success");
            }

            // Patch
            _mapper.Map(req, recruitment);
            await _recruitmentService.UpdateAsync(recruitment);
            await _userService.SaveChangesAsync();
            return new ApiResponse("Post Success");
        }

        [HttpDelete("{recruitmentId}")]
        [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> DeleteRecruitment(Guid recruitmentId)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
            var user = await _userService.CheckAndGetUserAsync(userId);
            await _recruitmentService.DeleteByIdAsync(recruitmentId);
            return new ApiResponse("刪除成功");
        }




    }

}

using AutoMapper;
using IngBackend.Services.UserService;
using IngBackend.Services.RecruitmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IngBackend.Models.DTO;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using IngBackend.Exceptions;
using IngBackend.Services.AreaService;
using AutoWrapper.Wrappers;

namespace IngBackend.Controllers
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
        public async Task<RecruitmentDTO> GetRecruitmentById(Guid recruitmentId)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

            var user = await _userService.CheckAndGetUserAsync(userId);
            var recruitment = _recruitmentService.GetRecruitmentIncludeAllById(recruitmentId);

            var recruitmentDTO = _mapper.Map<RecruitmentDTO>(recruitment);
            return recruitmentDTO;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseDTO<RecruitmentDTO>), StatusCodes.Status200OK)]
        public async Task<RecruitmentDTO> PostRecruitment([FromBody] RecruitmentPostDTO req)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
            var user = await _userService.CheckAndGetUserAsync(userId, u => u.Recruitments);

            var recruitment = await _recruitmentService.GetByIdAsync(req.Id ?? Guid.Empty);

            // Add new recruitment
            if (recruitment == null)
            {
                var newRecruitment = _mapper.Map<Recruitment>(req);
                newRecruitment.Publisher = user;
                user.Recruitments.Add(newRecruitment);
                _userService.Update(user);
                await _userService.SaveChangesAsync();
                return _mapper.Map<RecruitmentDTO>(newRecruitment);
            }

            // Patch
            _mapper.Map(req, recruitment);
            _userService.Update(user);
            await _userService.SaveChangesAsync();
            var recruitmentDTO = _mapper.Map<RecruitmentDTO>(recruitment);
            return recruitmentDTO;
        }

        [HttpDelete("{recruitmentId}")]
        [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> DeleteRecruitment(Guid recruitmentId)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
            var user = await _userService.CheckAndGetUserAsync(userId, u => u.Recruitments);

            if (user.Recruitments == null)
            {
                throw new NotFoundException("找不到職缺");
            }

            var existRecruitment = user.Recruitments.FirstOrDefault(x => x.Id == recruitmentId) ?? throw new NotFoundException("找不到職缺");

            user.Recruitments.Remove(existRecruitment);
            _userService.Update(user);
            await _userService.SaveChangesAsync();
            return new ApiResponse("刪除成功");
        }




    }

}
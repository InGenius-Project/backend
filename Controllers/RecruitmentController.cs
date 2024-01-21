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

        public RecruitmentController(IMapper mapper,AreaService areaService, UserService userService, RecruitmentService recruitmentService)
        {
            _mapper = mapper;
            _areaService = areaService;
            _userService = userService;
            _recruitmentService = recruitmentService;
        }


        [HttpGet]
        public async Task<List<RecruitmentDTO>> GetRecruitmentsByUser()
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
            await _userService.CheckAndGetUserAsync(userId, Enum.UserRole.Company);

            var recruitments = _recruitmentService.GetUserRecruitements(userId);

            var recruitmentsDTO = _mapper.Map<List<RecruitmentDTO>>(recruitments);
            return recruitmentsDTO;
        }


        [HttpGet("{recruitmentId}")]
        public async Task<ActionResult<RecruitmentDTO>> GetRecruitmentById(Guid recruitmentId)
        {
            var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

            var user = await _userService.CheckAndGetUserAsync(userId);
            var recruitment =  _recruitmentService.GetRecruitmentIncludeAllById(recruitmentId);

            var recruitmentDTO = _mapper.Map<RecruitmentDTO>(recruitment);
            return recruitmentDTO;
        }

        [HttpPost]
        public async Task<ActionResult<RecruitmentDTO>> PostRecruitment([FromBody] RecruitmentPostDTO req)
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
        public async Task<IActionResult> DeleteRecruitment(Guid recruitmentId)
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
            return Ok();
        }




    }

}
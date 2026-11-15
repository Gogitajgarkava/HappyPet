using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/reminders")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyReminders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _reminderService.GetMyAsync(
                CurrentUserId.Value,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("run")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Run(CancellationToken cancellationToken)
        {
            var result = await _reminderService.RunAsync(cancellationToken);
            return Ok(result);
        }

        private int? CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : null;
            }
        }
    }
}

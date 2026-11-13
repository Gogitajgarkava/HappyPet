using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/operator")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Veterinarian) + "," + nameof(UserRole.Admin))]
    public class OperatorController : ControllerBase
    {
        private readonly IOperatorService _operatorService;

        public OperatorController(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            var result = await _operatorService.GetDashboardAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransfers(
            [FromQuery] OwnershipTransferStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _operatorService.GetTransfersAsync(
                status,
                search,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("transfers/{id:int}")]
        public async Task<IActionResult> GetTransfer(int id, CancellationToken cancellationToken)
        {
            var result = await _operatorService.GetTransferAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("transfers/{id:int}/mark-synced")]
        public async Task<IActionResult> MarkSynced(
            int id,
            [FromBody] MarkOwnershipTransferSyncedRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _operatorService.MarkSyncedAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("transfers/{id:int}/reject")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] RejectOwnershipTransferRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _operatorService.RejectAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

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

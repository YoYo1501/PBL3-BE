using BackendAPI.Models.DTOs.Contract.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController(IContractService service) : ControllerBase
{
    private int GetStudentId()
    {
        var studentIdStr = User.FindFirstValue("StudentId");
        return string.IsNullOrEmpty(studentIdStr) ? 2 : int.Parse(studentIdStr);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyContract()
    {
        var studentId = GetStudentId();
        var (success, message, data) = await service.GetMyContractAsync(studentId);
        if (!success) return NotFound(new { message });
        return Ok(new { message, data });
    }

    [HttpGet("renewal-packages")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetRenewalPackages()
    {
        var studentId = GetStudentId();
        var (success, message, packages) = await service.GetRenewalPackagesAsync(studentId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, packages });
    }

    [HttpPost("renew")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitRenewal([FromBody] RenewalRequestDto dto)
    {
        var studentId = GetStudentId();
        var (success, message) = await service.SubmitRenewalAsync(studentId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("renewals/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPending()
    {
        var list = await service.GetAllPendingRenewalsAsync();
        return Ok(list);
    }

    [HttpPut("renewals/{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRenewalDto dto)
    {
        var (success, message) = await service.ApproveRenewalAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllContracts([FromQuery] ContractListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize") ||
            Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("status"))
        {
            var paged = await service.GetPagedContractsAsync(query);
            return Ok(paged);
        }

        var list = await service.GetAllContractsAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetContractById(int id)
    {
        var contract = await service.GetContractByIdAsync(id);
        if (contract == null) return NotFound(new { message = "Hop dong khong ton tai." });
        return Ok(contract);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] UpdateContractRequestDto dto)
    {
        var (success, message) = await service.UpdateContractAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var (success, message) = await service.DeleteContractAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}

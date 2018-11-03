﻿using System.Threading.Tasks;
using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.DomainModel;
using Wormhole.Interface;

namespace Wormhole.Api.Controllers
{
    [Route("tenant")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ITenantLogic _tenantLogic;

        public TenantController(ITenantLogic tenantLogic)
        {
            _tenantLogic = tenantLogic;
        }

        [HttpPost("")]
        public async Task<IActionResult> AddTenant(AddTenantRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { Message = ErrorKeys.ParameterNull });
            }

            var input = Mapping.AutoMapper.Mapper.Map<Tenant>(request);
            var result = await _tenantLogic.AddTenant(input);

            if (result?.Error != null)
                return BadRequest(new { Message = result.Error });

            return Ok(ApiValidationResult.Ok());
        }

        [HttpPut("")]
        public async Task<IActionResult> EditTenant(EditTenantRequest request)
        {
            return null;
        }

        [HttpDelete("")]
        public async Task<IActionResult> DeleteTenant()
        {
            return null;
        }

    }
}
﻿using System;
using System.Threading.Tasks;
using hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;

namespace Wormhole.Api.Controllers
{
    [Route("output-channels")]
    [ApiController]
    public class OutputChannelController : ControllerBase
    {
        private readonly IOutputChannelDA _outputChannelDA;

        public OutputChannelController(IOutputChannelDA outputChannelDA)
        {
            _outputChannelDA = outputChannelDA;
        }

        [HttpPost("http-push")]
        public async Task<IActionResult> AddHttpPushOutputChannel(HttpPushOutputChannelAddRequest input)
        {
            var channel = Mapping.AutoMapper.Mapper.Map<OutputChannel>(input);
            await _outputChannelDA.AddOutputChannel(channel);
            var output = Mapping.AutoMapper.Mapper.Map<HttpPushOutputChannelAddResponse>(channel);
            return Ok(ApiValidatedResult<HttpPushOutputChannelAddResponse>.Ok(output));
        }

        [HttpPost("kafka")]
        public async Task<IActionResult> AddKafkaOutputChannel(KafkaOutputChannelAddRequest input)
        {
            var channel = Mapping.AutoMapper.Mapper.Map<OutputChannel>(input);
            await _outputChannelDA.AddOutputChannel(channel);
            var output = Mapping.AutoMapper.Mapper.Map<KafkaOutputChannelAddResponse>(channel);
            return Ok(ApiValidatedResult<KafkaOutputChannelAddResponse>.Ok(output));
        }
    }
}
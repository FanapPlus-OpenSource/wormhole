﻿using System.Threading.Tasks;
using Hydrogen.General.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wormhole.Api.Model.OutputChannels;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.DomainModel.OutputChannel;
using Wormhole.Interface;

namespace Wormhole.Api.Controllers
{
    [Route("output-channels")]
    [ApiController]
    public class OutputChannelController : ControllerBase
    {
        private readonly IOutputChannelLogic _outputChannelLogic;

        public OutputChannelController(IOutputChannelLogic outputChannelDa, ILogger<OutputChannelController> logger)
        {
            _outputChannelLogic = outputChannelDa;
            Logger = logger;
        }

        private ILogger<OutputChannelController> Logger { get; }

        [HttpPost("http-push")]
        public async Task<IActionResult> AddHttpPushOutputChannel(HttpPushOutputChannelAddRequest input)
        {
            Logger.LogDebug(
                $"{nameof(OutputChannelController)} - {nameof(AddHttpPushOutputChannel)} method called with this input: {input}");

            var channel = Mapping.AutoMapper.Mapper.Map<OutputChannel>(input);

            await _outputChannelLogic.Create(channel);

            var output = Mapping.AutoMapper.Mapper.Map<HttpPushOutputChannelAddResponse>(channel);

            return Ok(ApiValidatedResult<HttpPushOutputChannelAddResponse>.Ok(output));
        }

        [HttpPost("kafka")]
        public async Task<IActionResult> AddKafkaOutputChannel(KafkaOutputChannelAddRequest input)
        {
            Logger.LogDebug($"{nameof(OutputChannelController)} - {nameof(AddKafkaOutputChannel)} method called with this input: {input}");

            var channel = Mapping.AutoMapper.Mapper.Map<OutputChannel>(input);

            await _outputChannelLogic.Create(channel);

            var output = Mapping.AutoMapper.Mapper.Map<KafkaOutputChannelAddResponse>(channel);

            return Ok(ApiValidatedResult<KafkaOutputChannelAddResponse>.Ok(output));
        }
    }
}
﻿using AutoMapper;
using Wormhole.Api.Model;
using Wormhole.DomainModel;

namespace Wormhole.Mapping.Profile
{
    public class
        HttpPushOutputChannelAddResponseProfile : OutputChannelAddResponseProfile<HttpPushOutputChannelAddResponse>
    {
        public HttpPushOutputChannelAddResponseProfile()
        {
            Expression.ForMember(
                d => d.TargetUrl,
                opt => opt.MapFrom(src =>
                    ((HttpPushOutputChannelSpecification) src.TypeSpecification).TargetUrl
                ));
        }
    }
}

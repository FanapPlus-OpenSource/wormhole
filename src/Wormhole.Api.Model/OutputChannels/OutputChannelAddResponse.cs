﻿namespace Wormhole.Api.Model.OutputChannels
{
    public class OutputChannelAddResponse
    {
        public string ExternalKey { get; set; }
        public string TenantId { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
    }
}
using System;

namespace PinkParrot.Pipeline
{
    public sealed class AppFeature : IAppFeature
    {
        public Guid AppId { get; }

        public AppFeature(Guid appId)
        {
            AppId = appId;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public record EFAssetEntity : Asset
{
    public DomainId DocumentId { get; set; }

    public DomainId IndexedAppId { get; set; }
}

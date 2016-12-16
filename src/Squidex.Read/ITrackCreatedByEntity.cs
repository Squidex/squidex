using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Read
{
    public interface ITrackCreatedByEntity
    {
        RefToken CreatedBy { get; set; }
    }
}

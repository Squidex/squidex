// ==========================================================================
//  ISubjectCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface ISubjectCommand : ICommand
    {
        string SubjectId { get; set; }
    }
}

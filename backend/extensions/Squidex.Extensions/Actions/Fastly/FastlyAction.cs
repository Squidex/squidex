// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Fastly;

[RuleAction(
    Title = "Fastly",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 32'><path d='M10.68.948v1.736h.806v2.6A12.992 12.992 0 0 0 .951 18.051c0 7.178 5.775 12.996 12.9 12.996 7.124 0 12.9-5.819 12.9-12.996-.004-6.332-4.502-11.605-10.455-12.755l-.081-.013V2.684h.807V.948H10.68zm3.53 10.605c3.218.173 5.81 2.713 6.09 5.922v.211h-.734v.737h.734v.201c-.279 3.21-2.871 5.752-6.09 5.925v-.723h-.733v.721c-3.281-.192-5.905-2.845-6.077-6.152h.728v-.737h-.724c.195-3.284 2.808-5.911 6.073-6.103v.725h.733v-.727zm2.513 3.051l-2.462 2.282a1.13 1.13 0 0 0-.41-.078c-.633 0-1.147.517-1.147 1.155a1.15 1.15 0 0 0 1.147 1.155c.633 0 1.147-.517 1.147-1.155 0-.117-.018-.23-.05-.337l.002.008 2.223-2.505-.449-.526z'/></svg>",
    IconColor = "#e23335",
    Display = "Purge fastly cache",
    Description = "Remove entries from the fastly CDN cache.",
    ReadMore = "https://www.fastly.com/")]
public sealed record FastlyAction : RuleAction
{
    [LocalizedRequired]
    [Display(Name = "Api Key", Description = "The API key to grant access to Squidex.")]
    [Editor(RuleFieldEditor.Text)]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    [Display(Name = "Service Id", Description = "The ID of the fastly service.")]
    [Editor(RuleFieldEditor.Text)]
    public string ServiceId { get; set; }
}

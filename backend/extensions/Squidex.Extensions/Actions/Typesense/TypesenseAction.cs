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

namespace Squidex.Extensions.Actions.Typesense;

[RuleAction(
    Title = "Typesense",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 49.293 50.853'><path d='M15.074 15.493a8.19 8.19 0 0 1 .165 1.601c0 .479-.055.994-.165 1.546l-7.013-.055v18.552c0 1.546.718 2.32 2.154 2.32h4.196c.258.625.386 1.25.386 1.877 0 .625-.036 1.012-.11 1.159-1.693.22-3.442.331-5.245.331-3.57 0-5.356-1.527-5.356-4.582V18.585l-3.92.055A7.91 7.91 0 0 1 0 17.094c0-.515.055-1.049.166-1.601l3.92.055V9.751c0-.994.147-1.694.442-2.098.294-.442.865-.663 1.711-.663H7.73l.331.331v8.283z'/><path d='M18.296 40.848c.036-.81.257-1.693.662-2.65.442-.994.94-1.767 1.491-2.32 2.908 1.583 5.466 2.375 7.675 2.375 1.214 0 2.19-.24 2.926-.718.773-.479 1.16-1.123 1.16-1.933 0-1.288-.994-2.319-2.982-3.092l-3.092-1.16c-4.638-1.692-6.957-4.398-6.957-8.116 0-1.325.24-2.503.718-3.533a7.992 7.992 0 0 1 2.098-2.706c.92-.773 2.006-1.362 3.258-1.767 1.251-.405 2.65-.607 4.196-.607.7 0 1.472.055 2.32.165.882.11 1.766.277 2.65.497.883.184 1.73.405 2.54.663s1.508.534 2.097.828c0 .92-.184 1.877-.552 2.871-.368.994-.865 1.73-1.49 2.209-2.909-1.288-5.43-1.933-7.565-1.933-.957 0-1.712.24-2.264.718-.552.442-.828 1.03-.828 1.767 0 1.141.92 2.043 2.761 2.706l3.368 1.214c2.43.847 4.233 2.006 5.411 3.479 1.178 1.472 1.767 3.184 1.767 5.135 0 2.613-.976 4.711-2.927 6.294-1.95 1.546-4.748 2.32-8.392 2.32-3.57 0-6.92-.903-10.049-2.706z' style='fill:#fffff;fill-opacity:1' transform='translate(0 -.354)'/><path d='M45.373 50.687V.166A9.626 9.626 0 0 1 47.25 0c.736 0 1.417.055 2.042.166v50.521a11.8 11.8 0 0 1-2.042.166c-.7 0-1.326-.056-1.878-.166z'/></svg>",
    IconColor = "#1035bc",
    Display = "Populate Typesense index",
    Description = "Populate a full text search index in Typesense.",
    ReadMore = "https://www.elastic.co/")]
public sealed record TypesenseAction : RuleAction
{
    [AbsoluteUrl]
    [LocalizedRequired]
    [Display(Name = "Server Url", Description = "The url to the instance or cluster.")]
    [Editor(RuleFieldEditor.Url)]
    public Uri Host { get; set; }

    [LocalizedRequired]
    [Display(Name = "Index Name", Description = "The name of the index.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string IndexName { get; set; }

    [LocalizedRequired]
    [Display(Name = "Api Key", Description = "The api key.")]
    [Editor(RuleFieldEditor.Text)]
    public string ApiKey { get; set; }

    [Display(Name = "Document", Description = "The optional custom document.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the document.")]
    [Editor(RuleFieldEditor.Text)]
    public string Delete { get; set; }
}

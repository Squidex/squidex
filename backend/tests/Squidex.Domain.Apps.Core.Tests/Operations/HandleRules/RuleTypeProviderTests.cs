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

namespace Squidex.Domain.Apps.Core.Operations.HandleRules;

public class RuleTypeProviderTests
{
    private readonly RuleTypeProvider sut = new RuleTypeProvider();

    private abstract class MyRuleActionHandler : RuleActionHandler<MyRuleAction, string>
    {
        protected MyRuleActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }
    }

    public enum ActionEnum
    {
        Yes,
        No
    }

    [RuleAction(
        Title = "Action",
        IconImage = "<svg></svg>",
        IconColor = "#1e5470",
        Display = "Action display",
        Description = "Action description.",
        ReadMore = "https://www.readmore.com/")]
    public sealed record MyRuleAction : RuleAction
    {
        [LocalizedRequired]
        [Display(Name = "Url Name", Description = "Url Description")]
        [Editor(RuleFieldEditor.Url)]
        [Formattable]
        public Uri Url { get; set; }

        [Editor(RuleFieldEditor.Javascript)]
        public string Script { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public string Text { get; set; }

        [Editor(RuleFieldEditor.TextArea)]
        public string TextMultiline { get; set; }

        [Editor(RuleFieldEditor.Password)]
        public string Password { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public ActionEnum Enum { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public ActionEnum? EnumOptional { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public bool Boolean { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public bool? BooleanOptional { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public int Number { get; set; }

        [Editor(RuleFieldEditor.Text)]
        public int? NumberOptional { get; set; }
    }

    [Fact]
    public void Should_create_definition()
    {
        var expected = new RuleActionDefinition
        {
            Type = typeof(MyRuleAction),
            Title = "Action",
            IconImage = "<svg></svg>",
            IconColor = "#1e5470",
            Display = "Action display",
            Description = "Action description.",
            ReadMore = "https://www.readmore.com/"
        };

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "url",
            Display = "Url Name",
            Description = "Url Description",
            Editor = RuleFieldEditor.Url,
            IsFormattable = true,
            IsRequired = true
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "script",
            Display = "Script",
            Description = null,
            Editor = RuleFieldEditor.Javascript,
            IsRequired = false
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "text",
            Display = "Text",
            Description = null,
            Editor = RuleFieldEditor.Text,
            IsRequired = false
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "textMultiline",
            Display = "TextMultiline",
            Description = null,
            Editor = RuleFieldEditor.TextArea,
            IsRequired = false
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "password",
            Display = "Password",
            Description = null,
            Editor = RuleFieldEditor.Password,
            IsRequired = false
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "enum",
            Display = "Enum",
            Description = null,
            Editor = RuleFieldEditor.Dropdown,
            IsRequired = false,
            Options = new[] { "Yes", "No" }
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "enumOptional",
            Display = "EnumOptional",
            Description = null,
            Editor = RuleFieldEditor.Dropdown,
            IsRequired = false,
            Options = new[] { "Yes", "No" }
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "boolean",
            Display = "Boolean",
            Description = null,
            Editor = RuleFieldEditor.Checkbox,
            IsRequired = false
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "booleanOptional",
            Display = "BooleanOptional",
            Description = null,
            Editor = RuleFieldEditor.Checkbox,
            IsRequired = false
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "number",
            Display = "Number",
            Description = null,
            Editor = RuleFieldEditor.Number,
            IsRequired = true
        });

        expected.Properties.Add(new RuleActionProperty
        {
            Name = "numberOptional",
            Display = "NumberOptional",
            Description = null,
            Editor = RuleFieldEditor.Number,
            IsRequired = false
        });

        sut.Add<MyRuleAction>();

        var currentDefinition = sut.Actions.Values.First();

        currentDefinition.Should().BeEquivalentTo(expected);
    }
}

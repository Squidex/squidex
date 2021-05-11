// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class RuleElementRegistryTests
    {
        private readonly RuleRegistry sut = new RuleRegistry();

        private abstract class MyRuleActionHandler : RuleActionHandler<MyRuleAction, string>
        {
            protected MyRuleActionHandler(RuleEventFormatter formatter)
                : base(formatter)
            {
            }
        }

        [RuleAction(
            Title = "Invalid",
            IconImage = "<svg></svg>",
            IconColor = "#1e5470",
            Display = "Action display",
            Description = "Action description.",
            ReadMore = "https://www.readmore.com/")]
        public sealed record MyInvalidRuleAction : RuleAction
        {
            [DataType(DataType.Custom)]
            public string Custom { get; set; }
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
            [DataType(DataType.Url)]
            [Formattable]
            public Uri Url { get; set; }

            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [DataType(DataType.Text)]
            public string Text { get; set; }

            [DataType(DataType.MultilineText)]
            public string TextMultiline { get; set; }

            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Text)]
            public ActionEnum Enum { get; set; }

            [DataType(DataType.Text)]
            public ActionEnum? EnumOptional { get; set; }

            [DataType(DataType.Text)]
            public bool Boolean { get; set; }

            [DataType(DataType.Text)]
            public bool? BooleanOptional { get; set; }

            [DataType(DataType.Text)]
            public int Number { get; set; }

            [DataType(DataType.Text)]
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
                Editor = RuleActionPropertyEditor.Url,
                IsFormattable = true,
                IsRequired = true
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "email",
                Display = "Email",
                Description = null,
                Editor = RuleActionPropertyEditor.Email,
                IsRequired = false
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "text",
                Display = "Text",
                Description = null,
                Editor = RuleActionPropertyEditor.Text,
                IsRequired = false
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "textMultiline",
                Display = "TextMultiline",
                Description = null,
                Editor = RuleActionPropertyEditor.TextArea,
                IsRequired = false
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "password",
                Display = "Password",
                Description = null,
                Editor = RuleActionPropertyEditor.Password,
                IsRequired = false
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "enum",
                Display = "Enum",
                Description = null,
                Editor = RuleActionPropertyEditor.Dropdown,
                IsRequired = false,
                Options = new[] { "Yes", "No" }
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "enumOptional",
                Display = "EnumOptional",
                Description = null,
                Editor = RuleActionPropertyEditor.Dropdown,
                IsRequired = false,
                Options = new[] { "Yes", "No" }
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "boolean",
                Display = "Boolean",
                Description = null,
                Editor = RuleActionPropertyEditor.Checkbox,
                IsRequired = false
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "booleanOptional",
                Display = "BooleanOptional",
                Description = null,
                Editor = RuleActionPropertyEditor.Checkbox,
                IsRequired = false
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "number",
                Display = "Number",
                Description = null,
                Editor = RuleActionPropertyEditor.Number,
                IsRequired = true
            });

            expected.Properties.Add(new RuleActionProperty
            {
                Name = "numberOptional",
                Display = "NumberOptional",
                Description = null,
                Editor = RuleActionPropertyEditor.Number,
                IsRequired = false
            });

            sut.Add<MyRuleAction>();

            var currentDefinition = sut.Actions.Values.First();

            currentDefinition.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_throw_exception_if_validation_attribute_used_incorrectly()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Add<MyInvalidRuleAction>());
        }
    }
}

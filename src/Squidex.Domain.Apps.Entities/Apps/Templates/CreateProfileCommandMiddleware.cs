// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class CreateProfileCommandMiddleware : ICommandMiddleware
    {
        private const string TemplateName = "Profile";

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted && context.Command is CreateApp createApp && IsRightTemplate(createApp))
            {
                var appId = new NamedId<Guid>(createApp.AppId, createApp.Name);

                var publish = new Func<ICommand, Task>(command =>
                {
                    if (command is IAppCommand appCommand)
                    {
                        appCommand.AppId = appId;
                    }

                    return context.CommandBus.PublishAsync(command);
                });

                return Task.WhenAll(
                    CreateBasicsAsync(publish),
                    CreateProjectsSchemaAsync(publish),
                    CreateExperienceSchemaAsync(publish),
                    CreateSkillsSchemaAsync(publish),
                    CreateEducationSchemaAsync(publish),
                    CreatePublicationsSchemaAsync(publish),
                    CreateClientAsync(publish, appId.Id));
            }

            return next();
        }

        private static bool IsRightTemplate(CreateApp createApp)
        {
            return string.Equals(createApp.Template, TemplateName, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task CreateClientAsync(Func<ICommand, Task> publish, Guid appId)
        {
            await publish(new AttachClient { Id = "sample-client", AppId = appId });
        }

        private async Task CreateBasicsAsync(Func<ICommand, Task> publish)
        {
            var postsId = await CreateBasicsSchemaAsync(publish);

            await publish(new CreateContent
            {
                SchemaId = postsId,
                Data =
                    new NamedContentData()
                        .AddField("firstName",
                            new ContentFieldData()
                                .AddValue("iv", "John"))
                        .AddField("lastName",
                            new ContentFieldData()
                                .AddValue("iv", "Doe"))
                        .AddField("profession",
                            new ContentFieldData()
                                .AddValue("iv", "Software Developer")),
                Publish = true,
            });
        }

        private async Task<NamedId<Guid>> CreateBasicsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "basics",
                Properties = new SchemaProperties
                {
                    Label = "Basics"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "firstName",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "First Name",
                            Hints = "Your first name"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "lastName",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Last Name",
                            Hints = "Your last name"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "profession",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.TextArea,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Profession",
                            Hints = "Define your profession"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "image",
                        Properties = new AssetsFieldProperties
                        {
                            IsRequired = false,
                            IsListField = false,
                            MustBeImage = true,
                            Label = "Image",
                            Hints = "Your image"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "summary",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.TextArea,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Summary",
                            Hints = "Write a short summary about yourself"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "githubLink",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Github",
                            Hints = "An optional link to your Github account"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "blogLink",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Blog",
                            Hints = "An optional link to your blog"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "twitterLink",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Twitter",
                            Hints = "An optional link to your twitter account"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "linkedInLink",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "LinkedIn",
                            Hints = "An optional link to your LinkedIn account"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "emailAddress",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Email Address",
                            Hints = "An optional email address to contact you"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "legalTerms",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.TextArea,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Legal terms",
                            Hints = "The terms to fulfill legal requirements"
                        }
                    }
                },
                Publish = true
            };

            await publish(command);

            return new NamedId<Guid>(command.SchemaId, command.Name);
        }

        private async Task<NamedId<Guid>> CreateProjectsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "projects",
                Properties = new SchemaProperties
                {
                    Label = "Projects"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "name",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Name",
                            Hints = "The name of the projection"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "description",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.TextArea,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Description",
                            Hints = "Describe your project"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "image",
                        Properties = new AssetsFieldProperties
                        {
                            IsRequired = true,
                            IsListField = false,
                            MustBeImage = true,
                            Label = "Image",
                            Hints = "An image or screenshot for your project"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "label",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Label",
                            Hints = "An optional label to categorize your project, e.g. 'Open Source'"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "link",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "link",
                            Hints = "The logo of the company or organization you worked for"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "year",
                        Properties = new NumberFieldProperties
                        {
                            IsRequired = false,
                            IsListField = false,
                            Label = "Year",
                            Hints = "The year, when you realized the project, used for sorting only"
                        }
                    }
                },
                Publish = true
            };

            await publish(command);

            return new NamedId<Guid>(command.SchemaId, command.Name);
        }

        private async Task<NamedId<Guid>> CreateExperienceSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "experience",
                Properties = new SchemaProperties
                {
                    Label = "Experience"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "position",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Position",
                            Hints = "Your position in this job"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "company",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Company",
                            Hints = "The company or organization you worked for"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "logo",
                        Properties = new AssetsFieldProperties
                        {
                            IsRequired = false,
                            IsListField = false,
                            MustBeImage = true,
                            Label = "Logo",
                            Hints = "The logo of the company or organization you worked for"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "from",
                        Properties = new DateTimeFieldProperties
                        {
                            Editor = DateTimeFieldEditor.Date,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Start Date",
                            Hints = "The start date"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "to",
                        Properties = new DateTimeFieldProperties
                        {
                            Editor = DateTimeFieldEditor.Date,
                            IsRequired = false,
                            IsListField = false,
                            Label = "End Date",
                            Hints = "The end date, keep empty if you still work there"
                        }
                    }
                },
                Publish = true
            };

            await publish(command);

            return new NamedId<Guid>(command.SchemaId, command.Name);
        }

        private async Task<NamedId<Guid>> CreateEducationSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "education",
                Properties = new SchemaProperties
                {
                    Label = "Education"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "degree",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Degree",
                            Hints = "The degree you got or achieved"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "school",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "School",
                            Hints = "The school or university"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "logo",
                        Properties = new AssetsFieldProperties
                        {
                            IsRequired = false,
                            IsListField = false,
                            MustBeImage = true,
                            Label = "Logo",
                            Hints = "The logo of the school"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "from",
                        Properties = new DateTimeFieldProperties
                        {
                            Editor = DateTimeFieldEditor.Date,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Start Date",
                            Hints = "The start date"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "to",
                        Properties = new DateTimeFieldProperties
                        {
                            Editor = DateTimeFieldEditor.Date,
                            IsRequired = false,
                            IsListField = false,
                            Label = "End Date",
                            Hints = "The end date, keep empty if you still study there"
                        }
                    }
                },
                Publish = true
            };

            await publish(command);

            return new NamedId<Guid>(command.SchemaId, command.Name);
        }

        private async Task<NamedId<Guid>> CreatePublicationsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "publications",
                Properties = new SchemaProperties
                {
                    Label = "Publications"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "name",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Name",
                            Hints = "The name or title of your publication"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "description",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.TextArea,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Description",
                            Hints = "Describe the content of your publication"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "cover",
                        Properties = new AssetsFieldProperties
                        {
                            IsRequired = true,
                            IsListField = false,
                            MustBeImage = true,
                            Label = "Cover",
                            Hints = "The cover of your publication"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "link",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = false,
                            IsListField = false,
                            Label = "Link",
                            Hints = "An optional link to your publication"
                        }
                    }
                },
                Publish = true
            };

            await publish(command);

            return new NamedId<Guid>(command.SchemaId, command.Name);
        }

        private async Task<NamedId<Guid>> CreateSkillsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "skills",
                Properties = new SchemaProperties
                {
                    Label = "Skills"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "name",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            Label = "Name",
                            Hints = "The name for your skill"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "experience",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Dropdown,
                            IsRequired = true,
                            IsListField = true,
                            AllowedValues = ImmutableList.Create("Beginner", "Advanced", "Professional", "Expert"),
                            Label = "Experience",
                            Hints = "The level of experience"
                        }
                    }
                },
                Publish = true
            };

            await publish(command);

            return new NamedId<Guid>(command.SchemaId, command.Name);
        }
    }
}

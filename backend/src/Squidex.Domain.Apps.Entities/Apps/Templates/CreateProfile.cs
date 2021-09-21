// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps.Templates.Builders;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class CreateProfile : ITemplate
    {
        public string Name { get; } = "profile";

        public Task RunAsync(PublishTemplate publish)
        {
            return Task.WhenAll(
                CreateBasicsAsync(publish),
                CreateEducationSchemaAsync(publish),
                CreateExperienceSchemaAsync(publish),
                CreateProjectsSchemaAsync(publish),
                CreatePublicationsSchemaAsync(publish),
                CreateSkillsSchemaAsync(publish));
        }

        private static async Task CreateBasicsAsync(PublishTemplate publish)
        {
            var postsId = await CreateBasicsSchemaAsync(publish);

            await publish(new UpdateContent
            {
                ContentId = postsId.Id,
                Data =
                    new ContentData()
                        .AddField("firstName",
                            new ContentFieldData()
                                .AddInvariant("John"))
                        .AddField("lastName",
                            new ContentFieldData()
                                .AddInvariant("Doe"))
                        .AddField("profession",
                            new ContentFieldData()
                                .AddInvariant("Software Developer")),
                SchemaId = postsId
            });
        }

        private static async Task<NamedId<DomainId>> CreateBasicsSchemaAsync(PublishTemplate publish)
        {
            var command =
                SchemaBuilder.Create("Basics")
                    .Singleton()
                    .AddString("First Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("Your first name."))
                    .AddString("Last Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("Your last name."))
                    .AddAssets("Image", f => f
                        .Properties(p => p with
                        {
                            ExpectedType = AssetType.Image,
                            MaxItems = 1,
                            MinItems = 1
                        })
                        .Hints("Your profile image."))
                    .AddString("Profession", f => f
                        .Properties(p => p with
                        {
                            Editor = StringFieldEditor.TextArea
                        })
                        .Required()
                        .Hints("Describe your profession."))
                    .AddString("Summary", f => f
                        .Properties(p => p with
                        {
                            Editor = StringFieldEditor.TextArea
                        })
                        .Hints("Write a short summary about yourself."))
                    .AddString("Legal Terms", f => f
                        .Properties(p => p with
                        {
                            Editor = StringFieldEditor.TextArea
                        })
                        .Hints("The terms to fulfill legal requirements."))
                    .AddString("Github Link", f => f
                        .Hints("An optional link to your Github account."))
                    .AddString("Blog Link", f => f
                        .Hints("An optional link to your Blog."))
                    .AddString("Twitter Link", f => f
                        .Hints("An optional link to your Twitter account."))
                    .AddString("LinkedIn Link", f => f
                        .Hints("An optional link to your LinkedIn  account."))
                    .AddString("Email Address", f => f
                        .Hints("An optional email address to contact you."))
                    .Build();

            await publish(command);

            return NamedId.Of(command.SchemaId, command.Name);
        }

        private static async Task<NamedId<DomainId>> CreateProjectsSchemaAsync(PublishTemplate publish)
        {
            var schema =
                SchemaBuilder.Create("Projects")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The name of your project."))
                    .AddString("Description", f => f
                        .Properties(p => p with
                        {
                            Editor = StringFieldEditor.TextArea
                        })
                        .Required()
                        .Hints("Describe your project."))
                    .AddAssets("Image", f => f
                        .Properties(p => p with
                        {
                            ExpectedType = AssetType.Image,
                            MaxItems = 1,
                            MinItems = 1
                        })
                        .Required()
                        .Hints("An image or screenshot for your project."))
                    .AddString("Label", f => f
                        .Properties(p => p with { Editor = StringFieldEditor.TextArea })
                        .Hints("An optional label to categorize your project, e.g. 'Open Source'."))
                    .AddString("Link", f => f
                        .Hints("An optional link to your project."))
                    .AddNumber("Year", f => f
                        .Hints("The year, when you realized the project, used for sorting only."))
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }

        private static async Task<NamedId<DomainId>> CreateExperienceSchemaAsync(PublishTemplate publish)
        {
            var schema =
                SchemaBuilder.Create("Experience")
                    .AddString("Position", f => f
                        .Required()
                        .ShowInList()
                        .Hints("Your position in this job."))
                    .AddString("Company", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The company or organization you worked for."))
                    .AddAssets("Logo", f => f
                        .Properties(p => p with
                        {
                            ExpectedType = AssetType.Image,
                            MaxItems = 1,
                            MinItems = 1
                        })
                        .Hints("The logo of the company or organization you worked for."))
                    .AddDateTime("From", f => f
                        .Required()
                        .Hints("The start date."))
                    .AddDateTime("To", f => f
                        .Hints("The end date, keep empty if you still work there."))
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }

        private static async Task<NamedId<DomainId>> CreateEducationSchemaAsync(PublishTemplate publish)
        {
            var schema =
                SchemaBuilder.Create("Education")
                    .AddString("Degree", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The degree you got or achieved."))
                    .AddString("School", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The school or university."))
                    .AddAssets("Logo", f => f
                        .Properties(p => p with
                        {
                            ExpectedType = AssetType.Image,
                            MaxItems = 1,
                            MinItems = 1
                        })
                        .Hints("The logo of the school or university."))
                    .AddDateTime("From", f => f
                        .Required()
                        .Hints("The start date."))
                    .AddDateTime("To", f => f
                        .Hints("The end date, keep empty if you still study there."))
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }

        private static async Task<NamedId<DomainId>> CreatePublicationsSchemaAsync(PublishTemplate publish)
        {
            var command =
                SchemaBuilder.Create("Publications")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The name or title of your publication."))
                    .AddAssets("Cover", f => f
                        .Properties(p => p with
                        {
                            ExpectedType = AssetType.Image,
                            MaxItems = 1,
                            MinItems = 1
                        })
                        .Hints("The cover of your publication."))
                    .AddString("Description", f => f
                        .Hints("Describe the content of your publication."))
                    .AddString("Link", f => f
                        .Hints("Optional link to your publication."))
                    .Build();

            await publish(command);

            return NamedId.Of(command.SchemaId, command.Name);
        }

        private static async Task<NamedId<DomainId>> CreateSkillsSchemaAsync(PublishTemplate publish)
        {
            var command =
                SchemaBuilder.Create("Skills")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The name of the skill."))
                    .AddString("Experience", f => f
                        .Properties(p => p with
                        {
                            AllowedValues = ImmutableList.Create(
                                "Beginner",
                                "Advanced",
                                "Professional",
                                "Expert"),
                            Editor = StringFieldEditor.Dropdown,
                        })
                        .Required()
                        .ShowInList()
                        .Hints("The level of experience."))
                    .Build();

            await publish(command);

            return NamedId.Of(command.SchemaId, command.Name);
        }
    }
}

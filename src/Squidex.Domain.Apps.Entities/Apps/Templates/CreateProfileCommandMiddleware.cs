// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Templates.Builders;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class CreateProfileCommandMiddleware : ICommandMiddleware
    {
        private const string TemplateName = "Profile";

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted && context.Command is CreateApp createApp && IsRightTemplate(createApp))
            {
                var appId = NamedId.Of(createApp.AppId, createApp.Name);

                var publish = new Func<ICommand, Task>(command =>
                {
                    if (command is IAppCommand appCommand)
                    {
                        appCommand.AppId = appId;
                    }

                    return context.CommandBus.PublishAsync(command);
                });

                await Task.WhenAll(
                    CreateBasicsAsync(publish),
                    CreateEducationSchemaAsync(publish),
                    CreateExperienceSchemaAsync(publish),
                    CreateProjectsSchemaAsync(publish),
                    CreatePublicationsSchemaAsync(publish),
                    CreateSkillsSchemaAsync(publish),
                    CreateClientAsync(publish, appId.Id));
            }

            await next();
        }

        private static bool IsRightTemplate(CreateApp createApp)
        {
            return string.Equals(createApp.Template, TemplateName, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task CreateClientAsync(Func<ICommand, Task> publish, Guid appId)
        {
            await publish(new AttachClient { Id = "sample-client", AppId = appId });
        }

        private static async Task CreateBasicsAsync(Func<ICommand, Task> publish)
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
                Publish = true
            });
        }

        private static async Task<NamedId<Guid>> CreateBasicsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command =
                SchemaBuilder.Create("basics")
                    .AddString("First Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("Your first name."))
                    .AddString("Last Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("Your last name."))
                    .AddAssets("Image", f => f
                        .MustBeImage()
                        .Hints("Your profile image."))
                    .AddString("Profession", f => f
                        .AsTextArea()
                        .Required()
                        .Hints("Describe your profession."))
                    .AddString("Summary", f => f
                        .AsTextArea()
                        .Hints("Write a short summary about yourself."))
                    .AddString("Legal Terms", f => f
                        .AsTextArea()
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

        private static async Task<NamedId<Guid>> CreateProjectsSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("projects")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The name of your project."))
                    .AddString("Description", f => f
                        .AsTextArea()
                        .Required()
                        .Hints("Describe your project."))
                    .AddAssets("Image", f => f
                        .MustBeImage()
                        .Required()
                        .Hints("An image or screenshot for your project."))
                    .AddString("Label", f => f
                        .AsTextArea()
                        .Hints("An optional label to categorize your project, e.g. 'Open Source'."))
                    .AddString("Link", f => f
                        .Hints("An optional link to your project."))
                    .AddNumber("Year", f => f
                        .Hints("The year, when you realized the project, used for sorting only."))
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }

        private static async Task<NamedId<Guid>> CreateExperienceSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("experience")
                    .AddString("Position", f => f
                        .Required()
                        .ShowInList()
                        .Hints("Your position in this job."))
                    .AddString("Company", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The company or organization you worked for."))
                    .AddAssets("Logo", f => f
                        .MustBeImage()
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

        private static async Task<NamedId<Guid>> CreateEducationSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("Experience")
                    .AddString("Degree", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The degree you got or achieved."))
                    .AddString("School", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The school or university."))
                    .AddAssets("Logo", f => f
                        .MustBeImage()
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

        private static async Task<NamedId<Guid>> CreatePublicationsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command =
                SchemaBuilder.Create("Publications")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The name or title of your publication."))
                    .AddAssets("Cover", f => f
                        .MustBeImage()
                        .Hints("The cover of your publication."))
                    .AddString("Description", f => f
                        .Hints("Describe the content of your publication."))
                    .AddString("Link", f => f
                        .Hints("Optional link to your publication."))
                    .Build();

            await publish(command);

            return NamedId.Of(command.SchemaId, command.Name);
        }

        private static async Task<NamedId<Guid>> CreateSkillsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command =
                SchemaBuilder.Create("Skills")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The name of the skill."))
                    .AddString("Experience", f => f
                        .AsDropDown("Beginner", "Advanced", "Professional", "Expert")
                        .Required()
                        .ShowInList()
                        .Hints("The level of experience."))
                    .Build();

            await publish(command);

            return NamedId.Of(command.SchemaId, command.Name);
        }
    }
}

// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Templates.Builders;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class CreateIdentityCommandMiddleware : ICommandMiddleware
    {
        private const string TemplateName = "Identity";
        private const string NormalizeScript = @"
            var data = ctx.data;
            
            if (data.userName && data.userName.iv) {
                data.normalizedUserName = { iv: data.userName.iv.toUpperCase() };
            }
            
            if (data.email && data.email.iv) {
                data.normalizedEmail = { iv: data.email.iv.toUpperCase() };
            }

            replace(data);";

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
                    CreateApiResourcesSchemaAsync(publish),
                    CreateAuthenticationSchemeSchemaAsync(publish),
                    CreateClientsSchemaAsync(publish),
                    CreateIdentityResourcesSchemaAsync(publish),
                    CreateSettingsSchemaAsync(publish),
                    CreateUsersSchemaAsync(publish),
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
            await publish(new AttachClient { Id = "default", AppId = appId });
        }

        private static async Task<NamedId<Guid>> CreateAuthenticationSchemeSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("Authentication Schemes")
                    .AddString("Provider", f => f
                        .AsDropDown("Facebook", "Google", "Microsoft", "Twitter")
                        .Required()
                        .ShowInList()
                        .Hints("The name and type of the provider."))
                    .AddString("Client Id", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The client id that you must configure at the external provider."))
                    .AddString("Client Secret", f => f
                        .Required()
                        .Hints("The client secret that you must configure at the external provider."))
                    .AddTags("Scopes", f => f
                        .Hints("Additional scopes you want from the provider."))
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }

        private static Task CreateClientsSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("Clients")
                    .AddString("Client Id", f => f
                        .Required()
                        .Hints("Unique id of the client."))
                    .AddString("Client Name", f => f
                        .Localizable()
                        .Hints("Client display name (used for logging and consent screen)."))
                    .AddString("Client Uri", f => f
                        .Localizable()
                        .Hints("URI to further information about client (used on consent screen)."))
                    .AddAssets("Logo", f => f
                        .MustBeImage()
                        .Hints("URI to client logo (used on consent screen)."))
                    .AddTags("Client Secrets", f => f
                        .Hints("Client secrets - only relevant for flows that require a secret."))
                    .AddTags("Allowed Scopes", f => f
                        .Hints("Specifies the api scopes that the client is allowed to request."))
                    .AddTags("Allowed Grant Types", f => f
                        .Hints("Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit, Hybrid, ResourceOwner, ClientCredentials)."))
                    .AddTags("Redirect Uris", f => f
                        .Hints("Specifies allowed URIs to return tokens or authorization codes to"))
                    .AddTags("Post Logout Redirect Uris", f => f
                        .Hints("Specifies allowed URIs to redirect to after logout."))
                    .AddTags("Allowed Cors Origins", f => f
                        .Hints("Gets or sets the allowed CORS origins for JavaScript clients."))
                    .AddBoolean("Require Consent", f => f
                        .AsToggle()
                        .Hints("Specifies whether a consent screen is required."))
                    .AddBoolean("Allow Offline Access", f => f
                        .AsToggle()
                        .Hints("Gets or sets a value indicating whether to allow offline access."))
                    .Build();

            return publish(schema);
        }

        private static Task CreateSettingsSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("Settings").Singleton()
                    .AddString("Site Name", f => f
                        .Localizable()
                        .Hints("The name of your website."))
                    .AddAssets("Logo", f => f
                        .MustBeImage()
                        .Hints("Logo that is rendered in the header."))
                    .AddString("Footer Text", f => f
                        .Localizable()
                        .Hints("The optional footer text."))
                    .AddString("PrivacyPolicyUrl", f => f
                        .Localizable()
                        .Hints("The link to your privacy policies."))
                    .AddString("LegalUrl", f => f
                        .Localizable()
                        .Hints("The link to your legal information."))
                    .AddString("Email Confirmation Text", f => f
                        .AsTextArea()
                        .Localizable()
                        .Hints("The text for the confirmation email."))
                    .AddString("Email Confirmation Subject", f => f
                        .AsTextArea()
                        .Localizable()
                        .Hints("The subject for the confirmation email."))
                    .AddString("Email Password Reset Text", f => f
                        .AsTextArea()
                        .Localizable()
                        .Hints("The text for the password reset email."))
                    .AddString("Email Password Reset Subject", f => f
                        .AsTextArea()
                        .Localizable()
                        .Hints("The subject for the password reset email."))
                    .AddString("Terms of Service Url", f => f
                        .Localizable()
                        .Hints("The link to your tems of service."))
                    .AddString("Bootstrap Url", f => f
                        .Hints("The link to a custom bootstrap theme."))
                    .AddString("Styles Url", f => f
                        .Hints("The link to a stylesheet."))
                    .AddString("SMTP From", f => f
                        .Hints("The SMTP sender address."))
                    .AddString("SMTP Server", f => f
                        .Hints("The smpt server."))
                    .AddString("SMTP Username", f => f
                        .Hints("The username for your SMTP server."))
                    .AddString("SMTP Password", f => f
                        .Hints("The password for your SMTP server."))
                    .AddString("Google Analytics Id", f => f
                        .Hints("The id to your google analytics account."))
                    .Build();

            return publish(schema);
        }

        private static async Task CreateUsersSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("Users")
                    .AddString("Username", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The unique username to login."))
                    .AddString("Email", f => f
                        .Pattern(@"^[a-zA-Z0-9.!#$%&’*+\\/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:.[a-zA-Z0-9-]+)*$", "Must be an email address.")
                        .Required()
                        .ShowInList()
                        .Hints("The unique email to login."))
                    .AddString("Phone Number", f => f
                        .Hints("Phone number of the user."))
                    .AddTags("Roles", f => f
                        .Hints("The roles of the user."))
                    .AddJson("Claims", f => f
                        .Hints("The claims of the user."))
                    .AddBoolean("Email Confirmed", f => f
                        .AsToggle()
                        .Hints("Indicates if the email is confirmed."))
                    .AddBoolean("Phone Number Confirmed", f => f
                        .AsToggle()
                        .Hints("Indicates if the phone number is confirmed."))
                    .AddBoolean("LockoutEnabled", f => f
                        .AsToggle()
                        .Hints("Toggle on to lock out the user."))
                    .AddDateTime("Lockout End Date Utc", f => f
                        .AsDateTime()
                        .Disabled()
                        .Hints("Indicates when the lockout ends."))
                    .AddTags("Login Keys", f => f
                        .Disabled()
                        .Hints("Login information for querying."))
                    .AddJson("Logins", f => f
                        .Disabled()
                        .Hints("Login information."))
                    .AddJson("Tokens", f => f
                        .Disabled()
                        .Hints("Login tokens."))
                    .AddNumber("Access Failed Count", f => f
                        .Disabled()
                        .Hints("The number of failed login attempts."))
                    .AddString("Password Hash", f => f
                        .Disabled()
                        .Hints("The hashed password."))
                    .AddString("Normalized Email", f => f
                        .Disabled()
                        .Hints("The normalized email for querying."))
                    .AddString("Normalized Username", f => f
                        .Disabled()
                        .Hints("The normalized user name for querying."))
                    .AddString("Security Stamp", f => f
                        .Disabled()
                        .Hints("Internal security stamp"))
                    .Build();

            await publish(schema);

            var schemaId = NamedId.Of(schema.SchemaId, schema.Name);

            await publish(new ConfigureScripts
            {
                SchemaId = schemaId.Id,
                ScriptCreate = NormalizeScript,
                ScriptUpdate = NormalizeScript
            });
        }

        private static Task CreateApiResourcesSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("API Resources")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The unique name of the API."))
                    .AddString("Display Name", f => f
                        .Localizable()
                        .Hints("The display name of the API."))
                    .AddString("Description", f => f
                        .Localizable()
                        .Hints("The description name of the API."))
                    .AddTags("User Claims", f => f
                        .Hints("List of accociated user claims that should be included when this resource is requested."))
                    .Build();

            return publish(schema);
        }

        private static Task CreateIdentityResourcesSchemaAsync(Func<ICommand, Task> publish)
        {
            var schema =
                SchemaBuilder.Create("Identity Resources")
                    .AddString("Name", f => f
                        .Required()
                        .ShowInList()
                        .Hints("The unique name of the identity information."))
                    .AddString("Display Name", f => f
                        .Localizable()
                        .Hints("The display name of the identity information."))
                    .AddString("Description", f => f
                        .Localizable()
                        .Hints("The description name of the identity information."))
                    .AddTags("User Claims", f => f
                        .Hints("List of accociated user claims that should be included when this resource is requested."))
                    .AddBoolean("Required", f => f
                        .Hints("Specifies whether the user can de-select the scope on the consent screen."))
                    .Build();

            return publish(schema);
        }
    }
}

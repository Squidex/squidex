// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using Squidex.Translator.Processes;
using Squidex.Translator.State;

#pragma warning disable CA1822 // Mark members as static

namespace Squidex.Translator
{
    public class Commands
    {
        [Command(Name = "info", Description = "Shows information about the translator.")]
        public void Info()
        {
            var version = typeof(Commands).Assembly.GetName().Version;

            Console.WriteLine($"Squidex Translator Version v{version}");
        }

        [Command(Name = "translate", Description = "Translates different parts.")]
        [SubCommand]
        public class Translate
        {
            [Command(Name = "check-backend", Description = "Check backend files.")]
            public void CheckBackend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "backend");

                new CheckBackend(folder, service).Run();
            }

            [Command(Name = "check-frontend", Description = "Check frontend files.")]
            public void CheckFrontend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "frontend");

                new CheckFrontend(folder, service).Run();
            }

            [Command(Name = "backend", Description = "Translate backend files.")]
            public void Backend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "backend");

                new TranslateBackend(folder, service).Run();
            }

            [Command(Name = "templates", Description = "Translate angular templates.")]
            public void Templates(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "frontend");

                new TranslateTemplates(folder, service).Run(arguments.Report);
            }

            [Command(Name = "typescript", Description = "Translate typescript files.")]
            public void Typescript(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "frontend");

                new TranslateTypescript(folder, service).Run();
            }

            [Command(Name = "gen-backend", Description = "Generate the backend translations.")]
            public void GenerateBackend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "backend");

                new GenerateBackendResources(folder, service).Run();
            }

            [Command(Name = "gen-frontend", Description = "Generate the frontend translations.")]
            public void GenerateFrontend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "frontend");

                new GenerateFrontendResources(folder, service).Run();
            }

            [Command(Name = "clean-backend", Description = "Clean the backend translations.")]
            public void CleanBackend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "backend");

                Helper.CleanOtherLocales(service);

                service.Save();
            }

            [Command(Name = "clean-frontend", Description = "Clean the frontend translations.")]
            public void CleanFrontend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "frontend");

                Helper.CleanOtherLocales(service);

                service.Save();
            }

            [Command(Name = "gen-keys", Description = "Generate the keys for translations.")]
            public void GenerateBackendKeys(TranslateArguments arguments)
            {
                var (backendFolder, serviceBackend) = Setup(arguments, "backend");

                new GenerateKeys(backendFolder, serviceBackend, "backend_keys.json").Run();

                var (frontendFolder, frontendService) = Setup(arguments, "frontend");

                new GenerateKeys(frontendFolder, frontendService, "frontend_keys.json").Run();
            }

            [Command(Name = "migrate-backend", Description = "Migrate the backend files.")]
            public void MigrateBackend(TranslateArguments arguments)
            {
                var (_, service) = Setup(arguments, "backend");

                service.Migrate();
            }

            [Command(Name = "migrate-frontend", Description = "Migrate the frontend files.")]
            public void MigrateFrontend(TranslateArguments arguments)
            {
                var (_, service) = Setup(arguments, "frontend");

                service.Migrate();
            }

            private static (DirectoryInfo, TranslationService) Setup(TranslateArguments arguments, string fileName)
            {
                if (!Directory.Exists(arguments.Folder))
                {
                    throw new ArgumentException("Folder does not exist.");
                }

                var supportedLocales = new string[] { "en", "nl", "it", "zh" };

                var locales = supportedLocales;

                if (arguments.Locales != null && arguments.Locales.Any())
                {
                    locales = supportedLocales.Intersect(arguments.Locales).ToArray();
                }

                if (locales.Length == 0)
                {
                    locales = supportedLocales;
                }

                var translationsDirectory = new DirectoryInfo(Path.Combine(arguments.Folder, "backend", "i18n"));
                var translationsService = new TranslationService(translationsDirectory, fileName, locales, arguments.SingleWords);

                return (new DirectoryInfo(arguments.Folder), translationsService);
            }
        }

        [Validator(typeof(Validator))]
        public sealed class TranslateArguments : IArgumentModel
        {
            [Operand(Name = "folder", Description = "The squidex folder.")]
            public string Folder { get; set; }

            [Option(LongName = "single", ShortName = "s", Description = "Single words only.")]
            public bool SingleWords { get; set; }

            [Option(LongName = "report", ShortName = "r")]
            public bool Report { get; set; }

            [Option(LongName = "locale", ShortName = "l")]
            public IEnumerable<string> Locales { get; set; }

            public sealed class Validator : AbstractValidator<TranslateArguments>
            {
                public Validator()
                {
                    RuleFor(x => x.Folder).NotEmpty();
                }
            }
        }
    }
}

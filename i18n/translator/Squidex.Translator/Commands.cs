// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using CommandDotNet;
using FluentValidation;
using FluentValidation.Attributes;
using Squidex.Translator.Processes;
using Squidex.Translator.State;

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
                var (folder, service) = Setup(arguments, "translations-backend");

                new CheckBackend(folder, service).Run();
            }

            [Command(Name = "check-frontend", Description = "Check frontend files.")]
            public void CheckFrontend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "translations-frontend");

                new CheckFrontend(folder, service).Run();
            }

            [Command(Name = "backend", Description = "Translate backend files.")]
            public void Backend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "translations-backend");

                new TranslateBackend(folder, service).Run();
            }

            [Command(Name = "templates", Description = "Translate angular templates.")]
            public void Templates(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "translations-frontend");

                new TranslateTemplates(folder, service).Run(arguments.Report);
            }

            [Command(Name = "typescript", Description = "Translate typescript files.")]
            public void Typescript(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "translations-frontend");

                new TranslateTypescript(folder, service).Run();
            }

            [Command(Name = "gen-backend", Description = "Generate the backend translations.")]
            public void GenerateBackend(TranslateArguments arguments)
            {
                var (folder, service) = Setup(arguments, "translations-backend");

                new GenerateBackendResources(folder, service).Run();
            }

            [Command(Name = "migrate-backend", Description = "Migrate the backend files.")]
            public void MigrateBackend(TranslateArguments arguments)
            {
                var (_, service) = Setup(arguments, "translations-backend");

                service.Migrate();
            }

            [Command(Name = "migrate-frontend", Description = "Migrate the frontend files.")]
            public void MigrateFrontend(TranslateArguments arguments)
            {
                var (_, service) = Setup(arguments, "translations-frontend");

                service.Migrate();
            }

            private static (DirectoryInfo, TranslationService) Setup(TranslateArguments arguments, string file)
            {
                if (!Directory.Exists(arguments.Folder))
                {
                    throw new ArgumentException("Folder does not exist.");
                }

                var translationFile = new FileInfo(Path.Combine(arguments.Folder, "i18n", file));
                var translationFolder = new TranslationService(translationFile, arguments.SingleWords);

                return (new DirectoryInfo(arguments.Folder), translationFolder);
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

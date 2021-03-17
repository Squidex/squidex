// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;

namespace Squidex.Infrastructure.Translations
{
    public sealed class MissingKeys
    {
        private const string MissingFileName = "__missing.txt";
        private readonly object lockObject = new object();
        private readonly HashSet<string> missingTranslations;

        public MissingKeys()
        {
            if (File.Exists(MissingFileName))
            {
                var missing = File.ReadAllLines(MissingFileName);

                missingTranslations = new HashSet<string>(missing);
            }
            else
            {
                missingTranslations = new HashSet<string>();
            }
        }

        public void Log(string key)
        {
            lock (lockObject)
            {
                if (!missingTranslations.Add(key))
                {
                    File.AppendAllLines(MissingFileName, new[] { key });
                }
            }
        }
    }
}

// ==========================================================================
//  AppClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps
{
    public sealed class AppClient
    {
        private readonly string name;
        private readonly string secret;
        private readonly bool isReader;

        public AppClient(string secret, string name, bool isReader)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(secret, nameof(secret));

            this.name = name;
            this.secret = secret;
            this.isReader = isReader;
        }

        public AppClient Change(bool newIsReader, Func<string> message)
        {
            if (isReader == newIsReader)
            {
                var error = new ValidationError("Client has already the reader state.", "IsReader");

                throw new ValidationException(message(), error);
            }

            return new AppClient(secret, name, newIsReader);
        }

        public AppClient Rename(string newName, Func<string> message)
        {
            if (string.Equals(name, newName))
            {
                var error = new ValidationError("Client already has the name", "Id");

                throw new ValidationException(message(), error);
            }

            return new AppClient(secret, newName, isReader);
        }
    }
}

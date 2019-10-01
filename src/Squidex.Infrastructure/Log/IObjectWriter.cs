// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.Log
{
    public interface IObjectWriter
    {
        IObjectWriter WriteProperty(string property, string? value);

        IObjectWriter WriteProperty(string property, double value);

        IObjectWriter WriteProperty(string property, long value);

        IObjectWriter WriteProperty(string property, bool value);

        IObjectWriter WriteProperty(string property, TimeSpan value);

        IObjectWriter WriteProperty(string property, Instant value);

        IObjectWriter WriteObject(string property, Action<IObjectWriter> objectWriter);

        IObjectWriter WriteObject<T>(string property, T context, Action<T, IObjectWriter> objectWriter);

        IObjectWriter WriteArray(string property, Action<IArrayWriter> arrayWriter);

        IObjectWriter WriteArray<T>(string property, T context, Action<T, IArrayWriter> arrayWriter);

        string ToString();
    }
}

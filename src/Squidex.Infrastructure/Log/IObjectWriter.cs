// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public interface IObjectWriter
    {
        IObjectWriter WriteProperty(string property, string value);
        IObjectWriter WriteProperty(string property, double value);
        IObjectWriter WriteProperty(string property, long value);
        IObjectWriter WriteProperty(string property, bool value);

        IObjectWriter WriteProperty(string property, TimeSpan value);
        IObjectWriter WriteProperty(string property, DateTime value);
        IObjectWriter WriteProperty(string property, DateTimeOffset value);

        IObjectWriter WriteObject(string property, Action<IObjectWriter> objectWriter);
        IObjectWriter WriteArray(string property, Action<IArrayWriter> arrayWriter);
    }
}

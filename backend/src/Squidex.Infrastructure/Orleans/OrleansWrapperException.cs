// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using System.Text;

namespace Squidex.Infrastructure.Orleans
{
    [Serializable]
    public class OrleansWrapperException : Exception
    {
        public Type ExceptionType { get; }

        public OrleansWrapperException(Exception wrapped, Type exceptionType)
            : base(FormatMessage(wrapped, exceptionType))
        {
            ExceptionType = exceptionType;
        }

        protected OrleansWrapperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExceptionType = Type.GetType(info.GetString(nameof(ExceptionType))!)!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ExceptionType), ExceptionType.AssemblyQualifiedName);

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(Exception wrapped, Type exceptionType)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Wrapping exception of type {exceptionType}, because original exception is not serialized.");
            sb.AppendLine();
            sb.AppendLine("Original exception:");
            sb.AppendLine(wrapped.ToString());

            return sb.ToString();
        }
    }
}

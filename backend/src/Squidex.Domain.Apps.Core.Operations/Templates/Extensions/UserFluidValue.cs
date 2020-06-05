// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public sealed class UserFluidValue : FluidValue
    {
        private readonly IUser value;

        public override FluidValues Type { get; } = FluidValues.Object;

        public UserFluidValue(IUser value)
        {
            this.value = value;
        }

        protected override FluidValue GetValue(string name, TemplateContext context)
        {
            switch (name)
            {
                case "id":
                    return Create(value.Id);
                case "email":
                    return Create(value.Email);
                case "name":
                    return Create(value.DisplayName());
                default:
                    return Create(value.Claims.FirstOrDefault(x => string.Equals(name, x.Type, StringComparison.OrdinalIgnoreCase))?.Value);
            }
        }

        public override bool Equals(FluidValue other)
        {
            return other is UserFluidValue user && user.value.Id == value.Id;
        }

        public override bool ToBooleanValue()
        {
            return true;
        }

        public override decimal ToNumberValue()
        {
            return 0;
        }

        public override object ToObjectValue()
        {
            return new UserFluidValue(value);
        }

        public override string ToStringValue()
        {
            return value.Id;
        }

        public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        {
            writer.Write(value.Id);
        }
    }
}

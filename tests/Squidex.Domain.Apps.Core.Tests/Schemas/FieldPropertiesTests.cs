// ==========================================================================
//  FieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class FieldPropertiesTests
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();

        public static IEnumerable<FieldProperties> Properties
        {
            get
            {
                yield return new AssetsFieldProperties();
                yield return new BooleanFieldProperties();
                yield return new DateTimeFieldProperties();
                yield return new GeolocationFieldProperties();
                yield return new JsonFieldProperties();
                yield return new NumberFieldProperties();
                yield return new ReferencesFieldProperties();
                yield return new StringFieldProperties();
            }
        }

        public static IEnumerable<object[]> PropertiesData
        {
            get { return Properties.Select(x => new object[] { x }); }
        }

        [Theory]
        [MemberData(nameof(PropertiesData))]
        public void Should_set_or_freeze_sut(FieldProperties properties)
        {
            foreach (var property in properties.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                property.SetValue(properties, value);

                var result = property.GetValue(properties);

                Assert.Equal(value, result);
            }

            properties.Freeze();

            foreach (var property in properties.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        property.SetValue(properties, value);
                    }
                    catch (Exception ex)
                    {
                        throw ex.InnerException;
                    }
                });
            }
        }
    }
}

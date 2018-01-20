// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards.FieldProperties
{
    public class JsonFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new JsonFieldProperties();

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            Assert.Empty(errors);
        }
    }
}

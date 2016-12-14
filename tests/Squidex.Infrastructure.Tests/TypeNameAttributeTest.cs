using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure
{
    public class TypeNameAttributeTest
    {
        [Fact]
        public void Should_instantiate()
        {
            var attribute = new TypeNameAttribute("MyTypeName");

            Assert.Equal("MyTypeName", attribute.TypeName);
        }
    }
}

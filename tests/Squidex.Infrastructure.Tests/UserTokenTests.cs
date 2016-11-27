using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure
{
    public class UserTokenTests
    {
        [Fact]
        public void Should_parse_user_token_from_string()
        {
            var token = UserToken.Parse("")
        }
    }
}

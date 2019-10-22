// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var workflow = Workflow.Default;

            var serialized = workflow.SerializeAndDeserialize();
            var test = workflow.GetTransitions(Status.Archived);

            serialized.Should().BeEquivalentTo(workflow);
        }

        [Fact]
        public void Should_verify_roles_mapping_in_workflow_transition()
        {
            var source = new
            {
               Expression = "expression_1",
               Role = "role_1"
            };

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver()
            };

            var json = JsonConvert.SerializeObject(source, serializerSettings);

            var serialized = JsonConvert.DeserializeObject<JsonWorkflowTransition>(json);

            var result = serialized.ToTransition();

            Assert.Equal(1, result.Roles.Count);
            Assert.Equal(source.Role, result.Roles[0]);
        }
    }
}

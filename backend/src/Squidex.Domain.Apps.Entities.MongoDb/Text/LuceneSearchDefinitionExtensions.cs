// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public static class LuceneSearchDefinitionExtensions
{
    public static IAggregateFluent<TResult> Search<TResult>(
       this IAggregateFluent<TResult> aggregate,
       BsonDocument search)
    {
        const string OperatorName = "$search";

        var stage = new DelegatedPipelineStageDefinition<TResult, TResult>(
            OperatorName,
            serializer =>
            {
                var document = new BsonDocument(OperatorName, search);

                return new RenderedPipelineStageDefinition<TResult>(OperatorName, document, serializer);
            });

        return aggregate.AppendStage(stage);
    }

    private sealed class DelegatedPipelineStageDefinition<TInput, TOutput> : PipelineStageDefinition<TInput, TOutput>
    {
        private readonly Func<IBsonSerializer<TInput>, RenderedPipelineStageDefinition<TOutput>> renderer;

        public override string OperatorName { get; }

        public DelegatedPipelineStageDefinition(string operatorName,
            Func<IBsonSerializer<TInput>, RenderedPipelineStageDefinition<TOutput>> renderer)
        {
            this.renderer = renderer;

            OperatorName = operatorName;
        }

        public override RenderedPipelineStageDefinition<TOutput> Render(
            IBsonSerializer<TInput> inputSerializer,
            IBsonSerializerRegistry serializerRegistry,
            LinqProvider linqProvider)
        {
            return renderer(inputSerializer);
        }
    }
}

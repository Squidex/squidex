// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Kafka
{
    [RuleAction(
        Title = "Kafka",
        IconImage = "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 1000 1000' enable-background='new 0 0 1000 1000' xml:space='preserve'><g><path d = 'M674.2,552.7c-38.2,0-72.4,17-95.9,43.6l-60.1-42.5c6.5-17.4,10.1-36.4,10.1-56.1c0-19.5-3.6-38-9.6-55.2l59.9-42c23.5,26.4,57.5,43.4,95.7,43.4c70.4,0,127.7-57.2,127.7-127.7c0-70.4-57.2-127.7-127.7-127.7c-70.4,0-127.7,57.2-127.7,127.7c0,12.5,2,24.8,5.4,36.2l-60.1,42c-25-31.1-61.3-52.8-102.2-59.5v-72.2c57.9-12.3,101.5-63.7,101.5-125C491.1,67.2,433.8,10,363.4,10S235.7,67.2,235.7,137.7c0,60.6,42.5,111.3,99.3,124.5v73.1c-77.8,13.4-136.8,80.9-136.8,162.3c0,81.6,59.7,149.4,137.5,162.5v77.4c-57.2,12.5-100.4,63.7-100.4,124.8c0,70.4,57.2,127.7,127.7,127.7c70.4,0,128.1-57.2,128.1-127.9c0-61-43.2-112.2-100.4-124.8V660c40.2-6.7,75.6-27.9,100.4-58.4l60.4,42.7c-3.4,11.4-5.1,23.5-5.1,36c0,70.4,57.2,127.7,127.7,127.7c70.4,0,127.7-57.2,127.7-127.7C801.6,609.9,744.6,552.7,674.2,552.7L674.2,552.7z M674.2,253.9c34.2,0,61.9,27.7,61.9,61.9c0,34.2-27.7,61.9-61.9,61.9c-34.2,0-62.2-27.7-62.2-61.9C612,281.7,640,253.9,674.2,253.9L674.2,253.9z M301.2,137.7c0-34.2,27.7-61.9,61.9-61.9c34.2,0,61.9,27.7,61.9,61.9s-27.7,61.9-61.9,61.9C329,199.6,301.2,171.7,301.2,137.7L301.2,137.7z M425.1,862.1c0,34.2-27.7,61.9-61.9,61.9c-34.2,0-61.9-27.7-61.9-61.9c0-34.2,27.7-61.9,61.9-61.9C397.4,800.2,425.1,828.1,425.1,862.1L425.1,862.1z M363.2,584c-47.6,0-86.3-38.7-86.3-86.3c0-47.6,38.7-86.3,86.3-86.3c47.6,0,86.3,38.7,86.3,86.3C449.7,545.3,410.8,584,363.2,584L363.2,584z M674.2,742.5c-34.2,0-61.9-27.7-61.9-61.9c0-34.2,27.7-61.9,61.9-61.9c34.2,0,61.9,27.7,61.9,61.9C736.1,714.8,708.2,742.5,674.2,742.5L674.2,742.5z'/></g></svg>",
        IconColor = "#404244",
        Display = "Push to kafka",
        Description = "Connect to Kafka stream and push data to that stream.",
        ReadMore = "https://kafka.apache.org/quickstart")]
    public sealed record KafkaAction : RuleAction
    {
        [LocalizedRequired]
        [Display(Name = "Topic Name", Description = "The name of the topic.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string TopicName { get; set; }

        [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Payload { get; set; }

        [Display(Name = "Key", Description = "The message key, commonly used for partitioning.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string Key { get; set; }

        [Display(Name = "Partition Key", Description = "The partition key, only used when we don't want to define partiontionig with key.")]
        [Editor(RuleFieldEditor.Text)]
        [Formattable]
        public string PartitionKey { get; set; }

        [Display(Name = "Partition Count", Description = "Define the number of partitions for specific topic.")]
        [Editor(RuleFieldEditor.Text)]
        public int PartitionCount { get; set; }

        [Display(Name = "Headers (Optional)", Description = "The message headers in the format '[Key]=[Value]', one entry per line.")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Headers { get; set; }

        [Display(Name = "Schema (Optional)", Description = "Define a specific AVRO schema in JSON format.")]
        [Editor(RuleFieldEditor.TextArea)]
        public string Schema { get; set; }
    }
}

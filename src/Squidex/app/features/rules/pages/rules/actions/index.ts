/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AlgoliaActionComponent } from './algolia-action.component';
import { AzureQueueActionComponent } from './azure-queue-action.component';
import { ElasticSearchActionComponent } from './elastic-search-action.component';
import { FastlyActionComponent } from './fastly-action.component';
import { MediumActionComponent } from './medium-action.component';
import { SlackActionComponent } from './slack-action.component';
import { TweetActionComponent } from './tweet-action.component';
import { WebhookActionComponent } from './webhook-action.component';

export default {
    Algolia: AlgoliaActionComponent,
    AzureQueue: AzureQueueActionComponent,
    ElasticSearch: ElasticSearchActionComponent,
    Fastly: FastlyActionComponent,
    Medium: MediumActionComponent,
    Slack: SlackActionComponent,
    Tweet: TweetActionComponent,
    Webhook: WebhookActionComponent
};

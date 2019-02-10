/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    HelpComponent,
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
    AlgoliaActionComponent,
    AssetChangedTriggerComponent,
    AzureQueueActionComponent,
    ContentChangedTriggerComponent,
    DiscourseActionComponent,
    ElasticSearchActionComponent,
    EmailActionComponent,
    FastlyActionComponent,
    MediumActionComponent,
    PrerenderActionComponent,
    RuleElementComponent,
    RuleEventBadgeClassPipe,
    RuleEventsPageComponent,
    RulesPageComponent,
    RuleWizardComponent,
    SchemaChangedTriggerComponent,
    SlackActionComponent,
    TweetActionComponent,
    UsageTriggerComponent,
    WebhookActionComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: RulesPageComponent,
        children: [
            {
                path: 'events',
                component: RuleEventsPageComponent
            },
            {
                path: 'help',
                component: HelpComponent,
                data: {
                    helpPage: '05-integrated/rules'
                }
            }
        ]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AlgoliaActionComponent,
        AssetChangedTriggerComponent,
        AzureQueueActionComponent,
        ContentChangedTriggerComponent,
        DiscourseActionComponent,
        EmailActionComponent,
        ElasticSearchActionComponent,
        FastlyActionComponent,
        MediumActionComponent,
        PrerenderActionComponent,
        RuleElementComponent,
        RuleEventBadgeClassPipe,
        RuleEventsPageComponent,
        RulesPageComponent,
        RuleWizardComponent,
        SchemaChangedTriggerComponent,
        SlackActionComponent,
        TweetActionComponent,
        UsageTriggerComponent,
        WebhookActionComponent
    ]
})
export class SqxFeatureRulesModule { }
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
} from 'shared';

import {
    AlgoliaActionComponent,
    AssetChangedTriggerComponent,
    AzureQueueActionComponent,
    ContentChangedTriggerComponent,
    FastlyActionComponent,
    RuleEventsPageComponent,
    RulesPageComponent,
    RuleWizardComponent,
    SlackActionComponent,
    WebhookActionComponent,
    ElasticSearchActionComponent
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
                    helpPage: '06-integrated/rules'
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
        FastlyActionComponent,
        RuleEventsPageComponent,
        RulesPageComponent,
        RuleWizardComponent,
        ElasticSearchActionComponent,
        SlackActionComponent,
        WebhookActionComponent
    ]
})
export class SqxFeatureRulesModule { }
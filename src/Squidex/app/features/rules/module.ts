/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    ContentChangedTriggerComponent,
    RuleEventsPageComponent,
    RulesPageComponent,
    RuleWizardComponent,
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
        ContentChangedTriggerComponent,
        RuleEventsPageComponent,
        RulesPageComponent,
        RuleWizardComponent,
        WebhookActionComponent
    ]
})
export class SqxFeatureRulesModule { }
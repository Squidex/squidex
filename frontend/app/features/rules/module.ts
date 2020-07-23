/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HelpComponent, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AssetChangedTriggerComponent, CommentTriggerComponent, ContentChangedTriggerComponent, GenericActionComponent, RuleComponent, RuleElementComponent, RuleEventBadgeClassPipe, RuleEventsPageComponent, RuleIconComponent, RulesPageComponent, RuleWizardComponent, SchemaChangedTriggerComponent, UsageTriggerComponent } from './declarations';

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
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule
    ],
    declarations: [
        AssetChangedTriggerComponent,
        CommentTriggerComponent,
        ContentChangedTriggerComponent,
        GenericActionComponent,
        RuleComponent,
        RuleElementComponent,
        RuleEventBadgeClassPipe,
        RuleEventsPageComponent,
        RuleIconComponent,
        RulesPageComponent,
        RuleWizardComponent,
        SchemaChangedTriggerComponent,
        UsageTriggerComponent
    ]
})
export class SqxFeatureRulesModule {}
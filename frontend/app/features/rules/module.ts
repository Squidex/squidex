/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HelpComponent, RuleMustExistGuard, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AssetChangedTriggerComponent, CommentTriggerComponent, ContentChangedTriggerComponent, GenericActionComponent, RuleComponent, RuleElementComponent, RuleEventBadgeClassPipe, RuleEventsPageComponent, RuleIconComponent, RulesPageComponent, SchemaChangedTriggerComponent, UsageTriggerComponent } from './declarations';
import { RulePageComponent } from './pages/rule/rule-page.component';
import { FormattableInputComponent } from './shared/actions/formattable-input.component';

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
    }, {
        path: ':ruleId',
        component: RulePageComponent,
        canActivate: [RuleMustExistGuard],
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
        FormattableInputComponent,
        GenericActionComponent,
        RuleComponent,
        RuleElementComponent,
        RuleEventBadgeClassPipe,
        RuleEventsPageComponent,
        RuleIconComponent,
        RulePageComponent,
        RulesPageComponent,
        SchemaChangedTriggerComponent,
        UsageTriggerComponent
    ]
})
export class SqxFeatureRulesModule {}
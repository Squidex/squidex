/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HelpComponent, RuleMustExistGuard, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AssetChangedTriggerComponent, CommentTriggerComponent, ContentChangedTriggerComponent, GenericActionComponent, RuleClassPipe, RuleComponent, RuleElementComponent, RuleEventsPageComponent, RuleIconComponent, RuleSimulatorPageComponent, RulesPageComponent, RuleTransitionComponent, SchemaChangedTriggerComponent, SimulatedRuleEventStatusPipe, UsageTriggerComponent } from './declarations';
import { RuleEventComponent } from './pages/events/rule-event.component';
import { RulePageComponent } from './pages/rule/rule-page.component';
import { SimulatedRuleEventComponent } from './pages/simulator/simulated-rule-event.component';
import { FormattableInputComponent } from './shared/actions/formattable-input.component';

const routes: Routes = [
    {
        path: '',
        component: RulesPageComponent,
        children: [
            {
                path: 'events',
                component: RuleEventsPageComponent,
            },
            {
                path: 'simulator',
                component: RuleSimulatorPageComponent,
            },
            {
                path: 'help',
                component: HelpComponent,
                data: {
                    helpPage: '05-integrated/rules',
                },
            },
        ],
    }, {
        path: ':ruleId',
        component: RulePageComponent,
        canActivate: [RuleMustExistGuard],
        children: [
            {
                path: 'events',
                component: RuleEventsPageComponent,
            },
            {
                path: 'simulator',
                component: RuleSimulatorPageComponent,
            },
            {
                path: 'help',
                component: HelpComponent,
                data: {
                    helpPage: '05-integrated/rules',
                },
            },
        ],
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        AssetChangedTriggerComponent,
        CommentTriggerComponent,
        ContentChangedTriggerComponent,
        FormattableInputComponent,
        GenericActionComponent,
        RuleClassPipe,
        RuleComponent,
        RuleElementComponent,
        RuleEventComponent,
        RuleEventsPageComponent,
        RuleIconComponent,
        RulePageComponent,
        RuleSimulatorPageComponent,
        RuleTransitionComponent,
        RulesPageComponent,
        SchemaChangedTriggerComponent,
        SimulatedRuleEventComponent,
        SimulatedRuleEventStatusPipe,
        UsageTriggerComponent,
    ],
})
export class SqxFeatureRulesModule {}

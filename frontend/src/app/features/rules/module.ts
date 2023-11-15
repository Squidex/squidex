/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { HelpComponent, ruleMustExistGuard } from '@app/shared';
import { RuleEventsPageComponent } from './pages/events/rule-events-page.component';
import { RulePageComponent } from './pages/rule/rule-page.component';
import { RulesPageComponent } from './pages/rules/rules-page.component';
import { RuleSimulatorPageComponent } from './pages/simulator/rule-simulator-page.component';

export const RULES_ROUTES: Routes = [
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
        canActivate: [ruleMustExistGuard],
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

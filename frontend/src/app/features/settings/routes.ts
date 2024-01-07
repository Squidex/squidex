/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { HelpComponent, HistoryComponent } from '@app/shared';
import { AssetScriptsPageComponent } from './pages/asset-scripts/asset-scripts-page.component';
import { ClientsPageComponent } from './pages/clients/clients-page.component';
import { ContributorsPageComponent } from './pages/contributors/contributors-page.component';
import { JobsPageComponent } from './pages/jobs/jobs-page.component';
import { LanguagesPageComponent } from './pages/languages/languages-page.component';
import { MorePageComponent } from './pages/more/more-page.component';
import { PlansPageComponent } from './pages/plans/plans-page.component';
import { RolesPageComponent } from './pages/roles/roles-page.component';
import { SettingsPageComponent } from './pages/settings/settings-page.component';
import { TemplatesPageComponent } from './pages/templates/templates-page.component';
import { WorkflowsPageComponent } from './pages/workflows/workflows-page.component';
import { SettingsAreaComponent } from './settings-area.component';

export const SETTINGS_ROUTES: Routes = [
    {
        path: '',
        component: SettingsAreaComponent,
        children: [
            {
                path: 'more',
                component: MorePageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.general',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/more',
                        },
                    },
                ],
            },
            {
                path: 'backups',
                redirectTo: 'jobs',
            },
            {
                path: 'jobs',
                component: JobsPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/jobs',
                        },
                    },
                ],
            },
            {
                path: 'clients',
                component: ClientsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.clients',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/clients',
                        },
                    },
                ],
            },
            {
                path: 'contributors',
                component: ContributorsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.contributors',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/contributors',
                        },
                    },
                ],
            },
            {
                path: 'languages',
                component: LanguagesPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.languages',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/languages',
                        },
                    },
                ],
            },
            {
                path: 'settings',
                component: SettingsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.ui',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/settings',
                        },
                    },
                ],
            },
            {
                path: 'templates',
                component: TemplatesPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/templates',
                        },
                    },
                ],
            },
            {
                path: 'plans',
                component: PlansPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.plan',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/plans',
                        },
                    },
                ],
            },
            {
                path: 'roles',
                component: RolesPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.roles',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/roles',
                        },
                    },
                ],
            },
            {
                path: 'workflows',
                component: WorkflowsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.workflows',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/workflows',
                        },
                    },
                ],
            },
            {
                path: 'asset-scripts',
                component: AssetScriptsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.assetScripts',
                        },
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/asset-scripts',
                        },
                    },
                ],
            },
        ],
    },
];
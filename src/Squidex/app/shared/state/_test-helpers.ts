/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { Mock } from 'typemoq';

import {
    AppsState,
    AuthService,
    DateTime,
    Version
} from './../';

const app = 'my-app';
const creation = DateTime.today().addDays(-2);
const creator = 'me';
const modified = DateTime.now().addDays(-1);
const modifier = 'now-me';
const version = new Version('1');
const newVersion = new Version('2');

const appsState = Mock.ofType<AppsState>();

appsState.setup(x => x.appName)
    .returns(() => app);

appsState.setup(x => x.selectedApp)
    .returns(() => of(<any>{ name: app }));

appsState.setup(x => x.selectedValidApp)
    .returns(() => of(<any>{ name: app }));

const authService = Mock.ofType<AuthService>();

authService.setup(x => x.user)
    .returns(() => <any>{ id: modifier, token: modifier });

export const TestValues = {
    app,
    appsState,
    authService,
    creation,
    creator,
    modified,
    modifier,
    newVersion,
    version
};
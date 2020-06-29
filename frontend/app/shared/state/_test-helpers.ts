/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { StateSynchronizer, StateSynchronizerMap } from '@app/framework';
import { of, Subject } from 'rxjs';
import { Mock } from 'typemoq';
import { AppsState, AuthService, DateTime, Version } from './../';

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

appsState.setup(x => x.selectedAppOrNull)
    .returns(() => of(<any>{ name: app }));

appsState.setup(x => x.selectedApp)
    .returns(() => of(<any>{ name: app }));

const authService = Mock.ofType<AuthService>();

authService.setup(x => x.user)
    .returns(() => <any>{ id: modifier, token: modifier });

class DummySynchronizer implements StateSynchronizer, StateSynchronizerMap<any> {
    constructor(
        private readonly subject: Subject<any>
    ) {
    }

    public build() {
        return;
    }

    public mapTo<T extends object>(): StateSynchronizerMap<T> {
        return this;
    }

    public keep() {
        return this;
    }

    public withString() {
        return this;
    }

    public withStrings() {
        return this;
    }

    public withPager() {
        return this;
    }

    public withSynchronizer() {
        return this;
    }

    public whenSynced(action: () => void) {
        this.subject.subscribe(() => action());

        return this;
    }
}

function buildDummyStateSynchronizer(): { synchronizer: StateSynchronizer, trigger: () => void } {
    const subject = new Subject<any>();

    const synchronizer = new DummySynchronizer(subject);

    const trigger = () => {
        subject.next();
    };

    return { synchronizer, trigger };
}

export const TestValues = {
    app,
    appsState,
    authService,
    buildDummyStateSynchronizer,
    creation,
    creator,
    modified,
    modifier,
    newVersion,
    version
};
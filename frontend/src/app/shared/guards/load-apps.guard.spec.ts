/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AppsState } from '@app/shared/internal';
import { loadAppsGuard } from './load-apps.guard';

describe('LoadAppsGuard', () => {
    let appsState: IMock<AppsState>;

    beforeEach(() => {
        appsState = Mock.ofType<AppsState>();

        TestBed.configureTestingModule({ providers: [{ provide: AppsState, useValue: appsState.object }] });
    });

    bit('should load apps', async () => {
        appsState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(loadAppsGuard());

        expect(result).toBeTruthy();

        appsState.verify(x => x.load(), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
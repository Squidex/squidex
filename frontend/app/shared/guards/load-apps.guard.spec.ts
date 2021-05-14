/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AppsState } from '@app/shared';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { LoadAppsGuard } from './load-apps.guard';

describe('LoadAppsGuard', () => {
    let appsState: IMock<AppsState>;
    let appGuard: LoadAppsGuard;

    beforeEach(() => {
        appsState = Mock.ofType<AppsState>();
        appGuard = new LoadAppsGuard(appsState.object);
    });

    it('should load apps', () => {
        appsState.setup(x => x.load())
            .returns(() => of(null));

        let result = false;

        appGuard.canActivate().subscribe(value => {
            result = value;
        });

        expect(result).toBeTruthy();

        appsState.verify(x => x.load(), Times.once());
    });
});

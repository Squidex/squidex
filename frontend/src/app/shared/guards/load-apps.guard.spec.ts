/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AppsState } from '@app/shared/internal';
import { LoadAppsGuard } from './load-apps.guard';

describe('LoadAppsGuard', () => {
    let appsState: IMock<AppsState>;
    let appGuard: LoadAppsGuard;

    beforeEach(() => {
        appsState = Mock.ofType<AppsState>();
        appGuard = new LoadAppsGuard(appsState.object);
    });

    it('should load apps', async () => {
        appsState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(appGuard.canActivate());

        expect(result).toBeTruthy();

        appsState.verify(x => x.load(), Times.once());
    });
});

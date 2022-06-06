/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AppsState } from '@app/shared/internal';
import { UnsetAppGuard } from './unset-app.guard';

describe('UnsetAppGuard', () => {
    let appsState: IMock<AppsState>;
    let appGuard: UnsetAppGuard;

    beforeEach(() => {
        appsState = Mock.ofType<AppsState>();
        appGuard = new UnsetAppGuard(appsState.object);
    });

    it('should unselect app', async () => {
        appsState.setup(x => x.select(null))
            .returns(() => of(null));

        const result = await firstValueFrom(appGuard.canActivate());

        expect(result).toBeTruthy();

        appsState.verify(x => x.select(null), Times.once());
    });
});

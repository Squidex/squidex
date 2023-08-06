/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { UIState } from '@app/shared/internal';
import { LoadSettingsGuard } from './load-settings.guard';

describe('LoadAppsGuard', () => {
    let settingsState: IMock<UIState>;
    let settingsGuard: LoadSettingsGuard;

    beforeEach(() => {
        settingsState = Mock.ofType<UIState>();
        settingsGuard = new LoadSettingsGuard(settingsState.object);
    });

    it('should load apps', async () => {
        settingsState.setup(x => x.load())
            .returns(() => of(null as any));

        const result = await firstValueFrom(settingsGuard.canActivate());

        expect(result).toBeTruthy();

        settingsState.verify(x => x.load(), Times.once());
    });
});

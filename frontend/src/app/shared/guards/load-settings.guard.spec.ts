/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { UIState } from '@app/shared/internal';
import { loadSettingsGuard } from './load-settings.guard';

describe('LoadAppsGuard', () => {
    let uiState: IMock<UIState>;

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();

        TestBed.configureTestingModule({ providers: [{ provide: UIState, useValue: uiState.object }] });
    });

    bit('should load apps', async () => {
        uiState.setup(x => x.load())
            .returns(() => of(null as any));

        const result = await firstValueFrom(loadSettingsGuard());

        expect(result).toBeTruthy();

        uiState.verify(x => x.load(), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
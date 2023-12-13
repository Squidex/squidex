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
import { unsetAppGuard } from './unset-app.guard';

describe('UnsetAppGuard', () => {
    let appsState: IMock<AppsState>;

    beforeEach(() => {
        appsState = Mock.ofType<AppsState>();

        TestBed.configureTestingModule({ providers: [{ provide: AppsState, useValue: appsState.object }] });
    });

    bit('should unselect app', async () => {
        appsState.setup(x => x.select(null))
            .returns(() => of(null));

        const result = await firstValueFrom(unsetAppGuard());

        expect(result).toBeTruthy();

        appsState.verify(x => x.select(null), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}

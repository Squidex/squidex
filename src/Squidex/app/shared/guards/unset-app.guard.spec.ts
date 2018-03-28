/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock, Times } from 'typemoq';
import { Observable } from 'rxjs';

import { AppsState } from '@app/shared';

import { UnsetAppGuard } from './unset-app.guard';

describe('UnsetAppGuard', () => {
    let appsState: IMock<AppsState>;

    beforeEach(() => {
        appsState = Mock.ofType(AppsState);
    });

    it('should unselect app', () => {
        appsState.setup(x => x.selectApp(null))
            .returns(() => Observable.of(null));

        const guard = new UnsetAppGuard(appsState.object);

        let result = false;

        guard.canActivate(<any>{}, <any>{})
            .subscribe(value => {
                result = value;
            });

        expect(result).toBeTruthy();

        appsState.verify(x => x.selectApp(null), Times.once());
    });
});
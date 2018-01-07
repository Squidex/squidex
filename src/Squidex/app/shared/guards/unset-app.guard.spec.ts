/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock, Times } from 'typemoq';
import { Observable } from 'rxjs';

import { AppsStoreService } from 'shared';

import { UnsetAppGuard } from './unset-app.guard';

describe('UnsetAppGuard', () => {
    let appStoreService: IMock<AppsStoreService>;

    beforeEach(() => {
        appStoreService = Mock.ofType(AppsStoreService);
    });

    it('should unselect app', () => {
        appStoreService.setup(x => x.selectApp(null))
            .returns(() => Observable.of(false));

        const guard = new UnsetAppGuard(appStoreService.object);

        let result = false;

        guard.canActivate(<any>{}, <any>{})
            .subscribe(value => {
                result = value;
            });

        expect(result).toBeTruthy();

        appStoreService.verify(x => x.selectApp(null), Times.once());
    });
});
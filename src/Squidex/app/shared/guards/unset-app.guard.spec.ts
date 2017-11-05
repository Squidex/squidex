/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock, Times } from 'typemoq';

import { AppsStoreService } from 'shared';

import { UnsetAppGuard } from './unset-app.guard';

describe('UnsetAppGuard', () => {
    let appStoreService: IMock<AppsStoreService>;

    beforeEach(() => {
        appStoreService = Mock.ofType(AppsStoreService);
    });

    it('should unselect app', () => {
        const guard = new UnsetAppGuard(appStoreService.object);

        expect(guard.canActivate(<any>{}, <any>{})).toBeTruthy();

        appStoreService.verify(x => x.selectApp(null), Times.once());
    });
});
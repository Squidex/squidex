/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { IMock, Mock } from 'typemoq';
import { IResourceDto, ResourceLinkDto, UIService, UIState, UsersService } from '@app/shared/internal';
import { TestValues } from './_test-helpers';

describe('UIState', () => {
    const { app } = TestValues;

    const common = {
        key: 'xx',
        map: {
            overridenByShared: 'common1',
            overridenByUser: 'common2',
            onlyCommon: 'common3',
        },
        canCreateApps: true,
    };

    const shared = {
        map: {
            overridenByShared: 'shared1',
            onlyShared: 'shared2',
        },
        canCreateApps: true,
    };

    const user = {
        map: {
            overridenByUser: 'user1',
            onlyUser: 'user2',
        },
        canCustomize: true,
    };

    const resource: IResourceDto = {
        _links: {
            'admin/events': new ResourceLinkDto({ method: 'GET', href: '/api/events' }),
            'admin/restore': new ResourceLinkDto({ method: 'GET', href: '/api/restore' }),
            'admin/users': new ResourceLinkDto({ method: 'GET', href: '/api/users' }),
        },
    };

    let usersService: IMock<UsersService>;
    let uiService: IMock<UIService>;
    let uiState: UIState;

    beforeEach(() => {
        uiService = Mock.ofType<UIService>();

        uiService.setup(x => x.getCommonSettings())
            .returns(() => of(common));

        uiService.setup(x => x.getAppSharedSettings(app))
            .returns(() => of(shared));

        uiService.setup(x => x.getAppUserSettings(app))
            .returns(() => of(user));

        usersService = Mock.ofType<UsersService>();

        usersService.setup(x => x.getResources())
            .returns(() => of(resource));

        uiState = TestBed.runInInjectionContext(() => new UIState(uiService.object, usersService.object));
        uiState.load();
        uiState.loadApp(app);
    });

    it('should load settings', () => {
        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyCommon: 'common3',
                onlyShared: 'shared2',
                onlyUser: 'user2',
            },
            canCreateApps: true,
            canCustomize: true,
        });

        expect(uiState.snapshot.canReadEvents).toBeTruthy();
        expect(uiState.snapshot.canReadUsers).toBeTruthy();
        expect(uiState.snapshot.canRestore).toBeTruthy();
    });

    it('should add value to snapshot if set as common', () => {
        uiService.setup(x => x.putCommonSetting('root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.setCommon('root.nested', 123);

        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyCommon: 'common3',
                onlyShared: 'shared2',
                onlyUser: 'user2',
            },
            canCreateApps: true,
            canCustomize: true,
            root: {
                nested: 123,
            },
        });

        expect(uiState.get('root', {})()).toEqual({ nested: 123 });
        expect(uiState.get('root.nested', 0)()).toEqual(123);
        expect(uiState.get('root.notfound', 1337)()).toEqual(1337);
        uiService.verifyAll();
    });

    it('should add value to snapshot if set as app shared', () => {
        uiService.setup(x => x.putAppSharedSetting(app, 'root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.setAppShared('root.nested', 123);

        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyCommon: 'common3',
                onlyShared: 'shared2',
                onlyUser: 'user2',
            },
            canCreateApps: true,
            canCustomize: true,
            root: {
                nested: 123,
            },
        });

        expect(uiState.get('root', {})()).toEqual({ nested: 123 });
        expect(uiState.get('root.nested', 0)()).toEqual(123);
        expect(uiState.get('root.notfound', 1337)()).toEqual(1337);
        uiService.verifyAll();
    });

    it('should add value to snapshot if set as app user', () => {
        uiService.setup(x => x.putAppUserSetting(app, 'root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.setAppUser('root.nested', 123);

        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyCommon: 'common3',
                onlyShared: 'shared2',
                onlyUser: 'user2',
            },
            canCreateApps: true,
            canCustomize: true,
            root: {
                nested: 123,
            },
        });

        expect(uiState.get('root', {})()).toEqual({ nested: 123 });
        expect(uiState.get('root.nested', 0)()).toEqual(123);
        expect(uiState.get('root.notfound', 1337)()).toEqual(1337);
        uiService.verifyAll();
    });

    it('should remove value from snapshot and common settings if removed', () => {
        uiService.setup(x => x.deleteCommonSetting('map.onlyCommon'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.onlyCommon');

        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyShared: 'shared2',
                onlyUser: 'user2',
            },
            canCreateApps: true,
            canCustomize: true,
        });

        uiService.verifyAll();
    });

    it('should remove value from snapshot and app shared settings if removed', () => {
        uiService.setup(x => x.deleteAppSharedSetting(app, 'map.onlyShared'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.onlyShared');

        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyCommon: 'common3',
                onlyUser: 'user2',
            },
            canCreateApps: true,
            canCustomize: true,
        });

        uiService.verifyAll();
    });

    it('should remove value from snapshot and app user settings if removed', () => {
        uiService.setup(x => x.deleteAppUserSetting(app, 'map.onlyUser'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.onlyUser');

        const settings = uiState.settings();

        expect(settings).toEqual({
            key: 'xx',
            map: {
                overridenByShared: 'shared1',
                overridenByUser: 'user1',
                onlyCommon: 'common3',
                onlyShared: 'shared2',
            },
            canCreateApps: true,
            canCustomize: true,
        });

        uiService.verifyAll();
    });

    it('should return default value from signal if settings not loaded', () => {
        const uiStateUnloaded = TestBed.runInInjectionContext(() => new UIState(uiService.object, usersService.object));

        expect(uiStateUnloaded.getAppUser('map.onlyUser', 'default')()).toEqual('default');
    });

    it('should wait for app user settings to be loaded', () => {
        const uiStateUnloaded = TestBed.runInInjectionContext(() => new UIState(uiService.object, usersService.object));

        let value: string | undefined;
        uiStateUnloaded.waitForAppUser('map.onlyUser', 'default').subscribe(x => {
            value = x;
        });

        expect(value).toBeUndefined();

        uiStateUnloaded.loadApp(app);
        TestBed.tick();

        expect(value).toEqual('user2');
    });

    it('should wait for common settings to be loaded', () => {
        const uiStateUnloaded = TestBed.runInInjectionContext(() => new UIState(uiService.object, usersService.object));

        let value: string | undefined;
        uiStateUnloaded.waitForCommon('map.onlyCommon', 'default').subscribe(x => {
            value = x;
        });

        expect(value).toBeUndefined();

        uiStateUnloaded.load();
        TestBed.tick();

        expect(value).toEqual('common3');
    });
});

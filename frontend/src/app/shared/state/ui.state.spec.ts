/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, Mock } from 'typemoq';
import { ResourceLinks, UIService, UIState, UsersService } from '@app/shared/internal';
import { TestValues } from './_test-helpers';

describe('UIState', () => {
    const {
        app,
    } = TestValues;

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

    const resources: ResourceLinks = {
        'admin/events': { method: 'GET', href: '/api/events' },
        'admin/restore': { method: 'GET', href: '/api/restore' },
        'admin/users': { method: 'GET', href: '/api/users' },
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
            .returns(() => of({ _links: resources }));

        uiState = new UIState(uiService.object, usersService.object);
        uiState.load();
        uiState.loadApp(app);
    });

    it('should load settings', () => {
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

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
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

        uiService.setup(x => x.putCommonSetting('root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.setCommon('root.nested', 123);

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

        uiState.get('root', {}).subscribe(x => {
            expect(x).toEqual({ nested: 123 });
        });

        uiState.get('root.nested', 0).subscribe(x => {
            expect(x).toEqual(123);
        });

        uiState.get('root.notfound', 1337).subscribe(x => {
            expect(x).toEqual(1337);
        });

        uiService.verifyAll();
    });

    it('should add value to snapshot if set as app shared', () => {
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

        uiService.setup(x => x.putAppSharedSetting(app, 'root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.setAppShared('root.nested', 123);

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

        uiState.get('root', {}).subscribe(x => {
            expect(x).toEqual({ nested: 123 });
        });

        uiState.get('root.nested', 0).subscribe(x => {
            expect(x).toEqual(123);
        });

        uiState.get('root.notfound', 1337).subscribe(x => {
            expect(x).toEqual(1337);
        });

        uiService.verifyAll();
    });

    it('should add value to snapshot if set as app user', () => {
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

        uiService.setup(x => x.putAppUserSetting(app, 'root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.setAppUser('root.nested', 123);

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

        uiState.get('root', {}).subscribe(x => {
            expect(x).toEqual({ nested: 123 });
        });

        uiState.get('root.nested', 0).subscribe(x => {
            expect(x).toEqual(123);
        });

        uiState.get('root.notfound', 1337).subscribe(x => {
            expect(x).toEqual(1337);
        });

        uiService.verifyAll();
    });

    it('should remove value from snapshot and common settings if removed', () => {
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

        uiService.setup(x => x.deleteCommonSetting('map.onlyCommon'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.onlyCommon');

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
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

        uiService.setup(x => x.deleteAppSharedSetting(app, 'map.onlyShared'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.onlyShared');

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
        let settings: any;

        uiState.settings.subscribe(value => {
            settings = value;
        });

        uiService.setup(x => x.deleteAppUserSetting(app, 'map.onlyUser'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.onlyUser');

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
});

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
        appsState,
    } = TestValues;

    const common = {
        key: 'xx',
        map: {
            type: 'GSM',
            sizeX: 800,
            sizeY: 600,
        },
        canCreateApps: true,
    };

    const shared = {
        map: {
            type: 'GM', key: 'xyz',
        },
        canCreateApps: true,
    };

    const user = {
        map: {
            sizeX: 1000,
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

        uiService.setup(x => x.getSharedSettings(app))
            .returns(() => of(shared));

        uiService.setup(x => x.getUserSettings(app))
            .returns(() => of(user));

        usersService = Mock.ofType<UsersService>();

        usersService.setup(x => x.getResources())
            .returns(() => of({ _links: resources }));

        uiState = new UIState(appsState.object, uiService.object, usersService.object);
    });

    it('should load settings', () => {
        expect(uiState.snapshot.settings).toEqual({
            key: 'xx',
            map: {
                type: 'GM',
                sizeX: 1000,
                sizeY: 600,
                key: 'xyz',
            },
            canCreateApps: true,
            canCustomize: true,
        });

        expect(uiState.snapshot.canReadEvents).toBeTruthy();
        expect(uiState.snapshot.canReadUsers).toBeTruthy();
        expect(uiState.snapshot.canRestore).toBeTruthy();
    });

    it('should add value to snapshot if set as shared', () => {
        uiService.setup(x => x.putSharedSetting(app, 'root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.set('root.nested', 123);

        expect(uiState.snapshot.settings).toEqual({
            key: 'xx',
            map: {
                type: 'GM',
                sizeX: 1000,
                sizeY: 600,
                key: 'xyz',
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

    it('should add value to snapshot if set as user', () => {
        uiService.setup(x => x.putUserSetting(app, 'root.nested', 123))
            .returns(() => of({})).verifiable();

        uiState.set('root.nested', 123, true);

        expect(uiState.snapshot.settings).toEqual({
            key: 'xx',
            map: {
                type: 'GM',
                sizeX: 1000,
                sizeY: 600,
                key: 'xyz',
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

    it('should remove value from snapshot and shared settings if removed', () => {
        uiService.setup(x => x.deleteSharedSetting(app, 'map.key'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.key');

        expect(uiState.snapshot.settings).toEqual({
            key: 'xx',
            map: {
                type: 'GM',
                sizeX: 1000,
                sizeY: 600,
            },
            canCreateApps: true,
            canCustomize: true,
        });

        uiService.verifyAll();
    });

    it('should remove value from snapshot and user settings if removed', () => {
        uiService.setup(x => x.deleteUserSetting(app, 'map.sizeX'))
            .returns(() => of({})).verifiable();

        uiState.remove('map.sizeX');

        expect(uiState.snapshot.settings).toEqual({
            key: 'xx',
            map: {
                type: 'GM',
                sizeX: 800,
                sizeY: 600,
                key: 'xyz',
            },
            canCreateApps: true,
            canCustomize: true,
        });

        uiService.verifyAll();
    });
});

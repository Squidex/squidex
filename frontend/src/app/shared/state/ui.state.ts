/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { distinctUntilChanged, filter, map } from 'rxjs/operators';
import { defined, hasAnyLink, State, Types } from '@app/framework';
import { UIService } from './../services/ui.service';
import { UsersService } from './../services/users.service';
import { AppsState } from './apps.state';

type Settings = { canCreateApps?: boolean; canCreateTeams?: boolean; [key: string]: any };

interface Snapshot {
    // All common settings.
    settingsCommon: Settings;

    // All shared app settings.
    settingsShared?: Settings | null;

    // All user app settings.
    settingsUser?: Settings | null;

    // The merged settings of app and common settings.
    settings: Settings;

    // Indicates if the user can read events.
    canReadEvents?: boolean;

    // Indicates if the user can read users.
    canReadUsers?: boolean;

    // Indicates if the user can restore backups.
    canRestore?: boolean;

    // Indicates if the user can use at least one admin resource.
    canUserAdminResource?: boolean;
}

@Injectable()
export class UIState extends State<Snapshot> {
    public settings =
        this.project(x => x.settings);

    public settingsShared =
        this.project(x => x.settingsShared).pipe(filter(x => !!x));

    public settingsUser =
        this.project(x => x.settingsUser).pipe(filter(x => !!x));

    public canReadEvents =
        this.project(x => x.canReadEvents === true);

    public canReadUsers =
        this.project(x => x.canReadUsers === true);

    public canRestore =
        this.project(x => x.canRestore === true);

    public canUserAdminResource =
        this.project(x => x.canRestore === true || x.canReadUsers === true || x.canReadEvents === true);

    public get<T>(path: string, defaultValue: T) {
        return this.settings.pipe(map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getShared<T>(path: string, defaultValue: T) {
        return this.settingsShared.pipe(map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getUser<T>(path: string, defaultValue: T) {
        return this.settingsUser.pipe(map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly uiService: UIService,
        private readonly usersService: UsersService,
    ) {
        super({
            settings: {},
            settingsCommon: {},
        }, 'Setting');

        this.loadResources();
        this.loadCommon();

        appsState.selectedApp.pipe(defined())
            .subscribe(app => {
                this.load(app.name);
            });
    }

    private load(app: string) {
        this.next(s => ({
            ...s,
            settings: s.settingsCommon,
            settingsShared: undefined,
            settingsUser: undefined,
        }), 'Loading Done');

        this.uiService.getSharedSettings(app)
            .subscribe(payload => {
                this.next(s => updateSettings(s, { settingsShared: payload }), 'Loading Shared Success');
            });

        this.uiService.getUserSettings(app)
            .subscribe(payload => {
                this.next(s => updateSettings(s, { settingsUser: payload }), 'Loading User Success');
            });
    }

    private loadCommon() {
        this.uiService.getCommonSettings()
            .subscribe(payload => {
                this.next(s => updateSettings(s, { settingsCommon: payload }), 'Loading Common Done');
            });
    }

    private loadResources() {
        this.usersService.getResources()
            .subscribe(payload => {
                this.next({
                    canReadEvents: hasAnyLink(payload, 'admin/events'),
                    canReadUsers: hasAnyLink(payload, 'admin/users'),
                    canRestore: hasAnyLink(payload, 'admin/restore'),
                }, 'Loading Resources Done');
            });
    }

    public set(path: string, value: any, user = false) {
        if (user) {
            this.setUser(path, value);
        } else {
            this.setShared(path, value);
        }
    }

    private setUser(path: string, value: any) {
        const { key, current, root } = getContainer(this.snapshot.settingsUser, path);

        if (current && key) {
            this.uiService.putUserSetting(this.appName, path, value).subscribe();

            current[key] = value;

            this.next(s => updateSettings(s, { settingsUser: root }), 'Set User');
        }
    }

    private setShared(path: string, value: any) {
        const { key, current, root } = getContainer(this.snapshot.settingsShared, path);

        if (current && key) {
            this.uiService.putSharedSetting(this.appName, path, value).subscribe();

            current[key] = value;

            this.next(s => updateSettings(s, { settingsShared: root }), 'Set Shared');
        }
    }

    public remove(path: string) {
        return this.removeUser(path) || this.removeShared(path);
    }

    public removeUser(path: string) {
        const { key, current, root } = getContainer(this.snapshot.settingsUser, path);

        if (current && key && current[key]) {
            this.uiService.deleteUserSetting(this.appName, path).subscribe();

            delete current[key];

            this.next(s => updateSettings(s, { settingsUser: root }), 'Removed User');

            return true;
        }

        return false;
    }

    public removeShared(path: string) {
        const { key, current, root } = getContainer(this.snapshot.settingsShared, path);

        if (current && key && current[key]) {
            this.uiService.deleteSharedSetting(this.appName, path).subscribe();

            delete current[key];

            this.next(s => updateSettings(s, { settingsShared: root }), 'Removed Shared');

            return true;
        }

        return false;
    }
}

function getValue<T>(setting: Settings | undefined | null, path: string, defaultValue: T) {
    if (!setting) {
        return defaultValue;
    }

    const segments = path.split('.');

    let current = setting;

    for (const segment of segments) {
        const temp = current[segment];

        if (temp) {
            current[segment] = temp;
        } else {
            return defaultValue;
        }

        current = temp;
    }

    return <T><any>current;
}

function getContainer(settings: Settings | undefined | null, path: string) {
    const segments = path.split('.');

    let current = { ...settings };

    const root = current;

    if (segments.length > 0) {
        for (let i = 0; i < segments.length - 1; i++) {
            const segment = segments[i];

            let temp = current[segment];

            if (!temp) {
                temp = {};
            } else {
                temp = { ...temp };
            }

            current[segment] = temp;

            if (!Types.isObject(temp)) {
                return { key: null, current: null, root: null };
            }

            current = temp;
        }
    }

    return { key: segments[segments.length - 1], current, root };
}

function updateSettings(state: Snapshot, update: Partial<Snapshot>) {
    const settings = {};

    Types.mergeInto(settings, update.settingsCommon || state.settingsCommon);
    Types.mergeInto(settings, update.settingsShared || state.settingsShared);
    Types.mergeInto(settings, update.settingsUser || state.settingsUser);

    return { ...state, settings, ...update };
}

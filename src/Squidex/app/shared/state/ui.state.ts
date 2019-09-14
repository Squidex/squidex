/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { distinctUntilChanged, map } from 'rxjs/operators';

import {
    hasAnyLink,
    mergeInto,
    State,
    Types
} from '@app/framework';

import { AppsState } from './apps.state';

import { UIService, UISettingsDto } from './../services/ui.service';

import { UsersService } from './../services/users.service';

interface Snapshot {
    // All common settings.
    settingsCommon: object & any;

    // All shared app settings.
    settingsShared: object & any;

    // All user app settings.
    settingsUser: object & any;

    // The merged settings of app and common settings.
    settings: object & any;

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
        this.project(x => x.settingsShared);

    public settingsUser =
        this.project(x => x.settingsUser);

    public canReadEvents =
        this.project(x => x.canReadEvents === true);

    public canReadUsers =
        this.project(x => x.canReadUsers === true);

    public canRestore =
        this.project(x => x.canRestore === true);

    public canUserAdminResource =
        this.project(x => x.canRestore === true || x.canReadUsers === true || x.canReadEvents === true);

    public get<T>(path: string, defaultValue: T) {
        return this.settings.pipe(map(x => this.getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getShared<T>(path: string, defaultValue: T) {
        return this.settingsShared.pipe(map(x => this.getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getUser<T>(path: string, defaultValue: T) {
        return this.settingsUser.pipe(map(x => this.getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly uiService: UIService,
        private readonly usersService: UsersService
    ) {
        super({
            settings: {},
            settingsCommon: {},
            settingsShared: {},
            settingsUser: {}
        });

        this.loadResources();
        this.loadCommon();

        appsState.selectedValidApp.subscribe(app => {
            this.load(app.name);
        });
    }

    private load(app: string) {
        this.next(s => updateSettings(s, {}));

        this.uiService.getSharedSettings(app)
            .subscribe(payload => {
                this.next(s => updateSettings(s, { settingsShared: payload }));
            });

        this.uiService.getUserSettings(app)
            .subscribe(payload => {
                this.next(s => updateSettings(s, { settingsUser: payload }));
            });
    }

    private loadCommon() {
        this.uiService.getCommonSettings()
            .subscribe(payload => {
                this.next(s => updateSettings(s, { settingsCommon: payload }));
            });
    }

    private loadResources() {
        this.usersService.getResources()
            .subscribe(payload => {
                this.next(s => ({ ...s,
                    canReadEvents: hasAnyLink(payload, 'admin/events'),
                    canReadUsers: hasAnyLink(payload, 'admin/users'),
                    canRestore: hasAnyLink(payload, 'admin/restore')
                }));
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

            this.next(s => updateSettings(s, { settingsUser: root }));
        }
    }

    private setShared(path: string, value: any) {
        const { key, current, root } = getContainer(this.snapshot.settingsShared, path);

        if (current && key) {
            this.uiService.putSharedSetting(this.appName, path, value).subscribe();

            current[key] = value;

            this.next(s => updateSettings(s, { settingsShared: root }));
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

            this.next(s => updateSettings(s, { settingsUser: root }));

            return true;
        }

        return false;
    }

    public removeShared(path: string) {
        const { key, current, root } = getContainer(this.snapshot.settingsShared, path);

        if (current && key && current[key]) {
            this.uiService.deleteSharedSetting(this.appName, path).subscribe();

            delete current[key];

            this.next(s => updateSettings(s, { settingsShared: root }));

            return true;
        }

        return false;
    }

    private getValue<T>(setting: object & UISettingsDto, path: string, defaultValue: T) {
        const segments = path.split('.');

        let current = setting;

        for (let segment of segments) {
            let temp = current[segment];

            if (temp) {
                current[segment] = temp;
            } else {
                return defaultValue;
            }

            current = temp;
        }

        return <T><any>current;
    }

    private get appName() {
        return this.appsState.appName;
    }
}

function getContainer(settings: object, path: string) {
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

    mergeInto(settings, update.settingsCommon || state.settingsCommon);
    mergeInto(settings, update.settingsShared || state.settingsShared);
    mergeInto(settings, update.settingsUser || state.settingsUser);

    return { ...state, settings, ...update };
}
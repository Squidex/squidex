/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { combineLatest } from 'rxjs';
import { distinctUntilChanged, filter, map, tap } from 'rxjs/operators';
import { debug, hasAnyLink, shareSubscribed, State, Types } from '@app/framework';
import { UIService } from '../services/ui.service';
import { UsersService } from '../services/users.service';

type Settings = { canCreateApps?: boolean; canCreateTeams?: boolean; [key: string]: any };

interface Snapshot {
    // All common settings.
    settingsCommon?: Settings | null;

    // All shared app settings.
    settingsAppShared?: Settings | null;

    // All user app settings.
    settingsAppUser?: Settings | null;

    // Indicates if the user can read events.
    canReadEvents?: boolean;

    // Indicates if the user can read users.
    canReadUsers?: boolean;

    // Indicates if the user can restore backups.
    canRestore?: boolean;

    // Indicates if the user can use at least one admin resource.
    canUseAdminResource?: boolean;

    // The app name.
    appName?: string;
}

@Injectable({
    providedIn: 'root',
})
export class UIState extends State<Snapshot> {
    public settings =
        this.project(mergeSettings);

    public settingsCommon =
        this.project(x => x.settingsCommon);

    public settingsShared =
        this.project(x => x.settingsAppShared).pipe(filter(x => !!x));

    public settingsUser =
        this.project(x => x.settingsAppUser).pipe(filter(x => !!x));

    public canReadEvents =
        this.project(x => x.canReadEvents === true);

    public canReadUsers =
        this.project(x => x.canReadUsers === true);

    public canRestore =
        this.project(x => x.canRestore === true);

    public canUseAdminResource =
        this.project(x => x.canRestore === true || x.canReadUsers === true || x.canReadEvents === true);

    public get<T>(path: string, defaultValue: T) {
        return this.settings.pipe(map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getCommon<T>(path: string, defaultValue: T) {
        return this.settingsCommon.pipe(filter(x => x !== undefined), map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getAppShared<T>(path: string, defaultValue: T) {
        return this.settingsShared.pipe(filter(x => x !== undefined), map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    public getAppUser<T>(path: string, defaultValue: T) {
        return this.settingsUser.pipe(filter(x => x !== undefined), map(x => getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    constructor(
        private readonly uiService: UIService,
        private readonly usersService: UsersService,
    ) {
        super({});

        debug(this, 'settings');
    }

    public load() {
        return combineLatest([this.loadCommon(), this.loadResources()]);
    }

    public loadApp(appName: string) {
        return combineLatest([this.loadAppShared(appName), this.loadAppUser(appName)]);
    }

    private loadAppUser(appName: string) {
        return this.uiService.getAppUserSettings(appName).pipe(
            tap(settingsAppUser => {
                this.next({ settingsAppUser, appName }, 'Loading App User Done');
            }),
            shareSubscribed(undefined, { throw: true }));
    }

    private loadAppShared(appName: string) {
        return this.uiService.getAppSharedSettings(appName).pipe(
            tap(settingsAppShared => {
                this.next({ settingsAppShared, appName }, 'Loading App Shared Done');
            }),
            shareSubscribed(undefined, { throw: true }));
    }

    private loadCommon() {
        return this.uiService.getCommonSettings().pipe(
            tap(payload => {
                this.next({ settingsCommon: payload }, 'Loading Common Done');
            }),
            shareSubscribed(undefined, { throw: true }));
    }

    private loadResources() {
        return this.usersService.getResources().pipe(
            tap(payload => {
                this.next({
                    canReadEvents: hasAnyLink(payload, 'admin/events'),
                    canReadUsers: hasAnyLink(payload, 'admin/users'),
                    canRestore: hasAnyLink(payload, 'admin/restore'),
                }, 'Loading Resources Done');
            }),
            shareSubscribed(undefined, { throw: true }));
    }

    public setCommon(path: string, value: any) {
        const { key, current, root } = getContainer(this.snapshot.settingsCommon, path);

        if (current && key) {
            this.uiService.putCommonSetting(path, value).subscribe();

            current[key] = value;

            this.next({ settingsCommon: root }, 'Set Common');
        }
    }

    public setAppUser(path: string, value: any) {
        const { key, current, root } = getContainer(this.snapshot.settingsAppUser, path);

        if (current && key) {
            this.uiService.putAppUserSetting(this.appName, path, value).subscribe();

            current[key] = value;

            this.next({ settingsAppUser: root }, 'Set App user');
        }
    }

    public setAppShared(path: string, value: any) {
        const { key, current, root } = getContainer(this.snapshot.settingsAppShared, path);

        if (current && key) {
            this.uiService.putAppSharedSetting(this.appName, path, value).subscribe();

            current[key] = value;

            this.next({ settingsAppShared: root }, 'Set App Shared');
        }
    }

    public remove(path: string) {
        return this.removeCommon(path) || this.removeAppUser(path) || this.removeAppShared(path);
    }

    public removeCommon(path: string) {
        const { key, current, root } = getContainer(this.snapshot.settingsCommon, path);

        if (current && key && current[key]) {
            this.uiService.deleteCommonSetting(path).subscribe();

            delete current[key];

            this.next({ settingsCommon: root }, 'Remove Common');
            return true;
        }

        return false;
    }

    public removeAppUser(path: string) {
        const { key, current, root } = getContainer(this.snapshot.settingsAppUser, path);

        if (current && key && current[key]) {
            this.uiService.deleteAppUserSetting(this.appName, path).subscribe();

            delete current[key];

            this.next({ settingsAppUser: root }, 'Remove App User');
            return true;
        }

        return false;
    }

    public removeAppShared(path: string) {
        const { key, current, root } = getContainer(this.snapshot.settingsAppShared, path);

        if (current && key && current[key]) {
            this.uiService.deleteAppSharedSetting(this.appName, path).subscribe();

            delete current[key];

            this.next({ settingsAppShared: root }, 'Remove App Shared');
            return true;
        }

        return false;
    }

    private get appName() {
        return this.snapshot.appName!;
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

function mergeSettings(state: Snapshot): Settings {
    const result = {};

    Types.mergeInto(result, state.settingsCommon);
    Types.mergeInto(result, state.settingsAppShared);
    Types.mergeInto(result, state.settingsAppUser);

    return result;
}

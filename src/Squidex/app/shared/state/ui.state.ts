/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { distinctUntilChanged, map } from 'rxjs/operators';

import { State, Types } from '@app/framework';

import { AppsState } from './apps.state';

import { UIService, UISettingsDto } from './../services/ui.service';

interface Snapshot {
    settings: object & any;
}

@Injectable()
export class UIState extends State<Snapshot> {
    public settings =
        this.changes.pipe(map(x => x.settings),
            distinctUntilChanged());

    public get<T>(path: string, defaultValue: T) {
        return this.settings.pipe(map(x => this.getValue(x, path, defaultValue)),
            distinctUntilChanged());
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly uiService: UIService
    ) {
        super({ settings: { mapType: 'OSM' } });

        if (appsState.selectedApp && Types.isFunction(appsState.selectedApp.subscribe)) {
            appsState.selectedApp.subscribe(app => {
                if (app) {
                    this.load(true);
                }
            });
        } else {
            this.load(true);
        }
    }

    public load(reset = false) {
        if (!reset) {
            this.resetState();
        }

        this.uiService.getSettings(this.appName)
            .subscribe(dtos => {
                return this.next({ settings: dtos });
            });
    }

    public set(path: string, value: any) {
        const { key, current, root } = this.getContainer(path);

        if (current && key) {
            this.uiService.putSetting(this.appName, path, value).subscribe();

            current[key] = value;

            this.next({ settings: root });
        }
    }

    public remove(path: string) {
        const { key, current, root } = this.getContainer(path);

        if (current && key) {
            this.uiService.deleteSetting(this.appName, path).subscribe();

            delete current[key];

            this.next({ settings: root });
        }
    }

    private getContainer(path: string) {
        const segments = path.split('.');

        let current = { ...this.snapshot.settings };

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
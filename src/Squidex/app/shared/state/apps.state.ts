
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import '@app/framework/utils/rxjs-extensions';

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
    DateTime,
    ImmutableArray,
    State
} from '@app/framework';

import {
    AppDto,
    AppsService,
    CreateAppDto
} from './../services/apps.service';

interface Snapshot {
    apps: ImmutableArray<AppDto>;

    selectedApp: AppDto | null;
}

@Injectable()
export class AppsState extends State<Snapshot> {
    public get appName() {
        return this.snapshot.selectedApp!.name;
    }

    public selectedApp =
        this.changes.map(s => s.selectedApp);

    public apps =
        this.changes.map(s => s.apps);

    constructor(
        private readonly appsService: AppsService
    ) {
        super({ apps: ImmutableArray.empty(), selectedApp: null });
    }

    public selectApp(name: string | null): Observable<AppDto | null> {
        const observable =
            !name ?
                Observable.of(null) :
                Observable.of(this.snapshot.apps.find(x => x.name === name) || null);

        return observable
            .do(selectedApp => {
                this.next(s => {
                    return { ...s, selectedApp };
                });
            });
    }

    public loadApps(): Observable<any> {
        return this.appsService.getApps()
            .do(dtos => {
                this.next(s => {
                    return { ...s, apps: ImmutableArray.of(dtos) };
                });
            });
    }

    public createApp(request: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appsService.postApp(request)
            .do(dto => {
                this.next(s => {
                    return { ...s, apps: s.apps.push(dto).sortByStringAsc(x => x.name) };
                });
            });
    }

    public deleteApp(name: string): Observable<any> {
        return this.appsService.deleteApp(name)
            .do(app => {
                this.next(s => {
                    const selectedApp = s.selectedApp && s.selectedApp.name === name ? null : s.selectedApp;

                    return { apps: s.apps.filter(x => x.name !== name), selectedApp };
                });
            });
    }
}
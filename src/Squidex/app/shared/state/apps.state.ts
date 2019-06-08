/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { distinctUntilChanged, filter, map, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State
} from '@app/framework';

import {
    AppCreatedDto,
    AppDto,
    AppsService,
    CreateAppDto
} from './../services/apps.service';

interface Snapshot {
    // All apps, loaded once.
    apps: ImmutableArray<AppDto>;

    // The selected app.
    selectedApp: AppDto | null;
}

function sameApp(lhs: AppDto, rhs?: AppDto): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id);
}

@Injectable()
export class AppsState extends State<Snapshot> {
    public get appName() {
        return this.snapshot.selectedApp ? this.snapshot.selectedApp.name : '';
    }

    public selectedApp =
        this.changes.pipe(map(s => s.selectedApp),
            distinctUntilChanged(sameApp));

    public selectedValidApp =
        this.selectedApp.pipe(filter(x => !!x), map(x => <AppDto>x),
            distinctUntilChanged());

    public apps =
        this.changes.pipe(map(s => s.apps),
            distinctUntilChanged());

    constructor(
        private readonly appsService: AppsService,
        private readonly dialogs: DialogService
    ) {
        super({ apps: ImmutableArray.empty(), selectedApp: null });
    }

    public select(name: string | null): Observable<AppDto | null> {
        const observable =
            !name ?
                of(null) :
                of(this.snapshot.apps.find(x => x.name === name) || null);

        return observable.pipe(
            tap(selectedApp => {
                this.next(s => ({ ...s, selectedApp }));
            }));
    }

    public load(): Observable<any> {
        return this.appsService.getApps().pipe(
            tap(payload => {
                this.next(s => {
                    const apps = ImmutableArray.of(payload);

                    return { ...s, apps };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appsService.postApp(request).pipe(
            map(payload => createApp(request, payload, now)),
            tap(created => {
                this.next(s => {
                    const apps = s.apps.push(created).sortByStringAsc(x => x.name);

                    return { ...s, apps };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public delete(name: string): Observable<any> {
        return this.appsService.deleteApp(name).pipe(
            tap(() => {
                this.next(s => {
                    const apps = s.apps.filter(x => x.name !== name);

                    const selectedApp = s.selectedApp && s.selectedApp.name === name ? null : s.selectedApp;

                    return { ...s, apps, selectedApp };
                });
            }),
            shareSubscribed(this.dialogs));
    }
}

function createApp(request: CreateAppDto, response: AppCreatedDto, now?: DateTime) {
    now = now || DateTime.now();

    const app = new AppDto(
        response.id,
        request.name,
        response.permissions,
        now,
        now,
        response.planName,
        response.planUpgrade);

    return app;
}
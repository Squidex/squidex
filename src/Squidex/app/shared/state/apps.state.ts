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
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State
} from '@app/framework';

import {
    AppDto,
    AppsService,
    CreateAppDto,
    UpdateAppDto
} from './../services/apps.service';

interface Snapshot {
    // All apps, loaded once.
    apps: ImmutableArray<AppDto>;

    // The selected app.
    selectedApp: AppDto | null;
}

@Injectable()
export class AppsState extends State<Snapshot> {
    public get appName() {
        return this.snapshot.selectedApp ? this.snapshot.selectedApp.name : '';
    }

    public get appDisplayName() {
        return this.snapshot.selectedApp ? this.snapshot.selectedApp.displayName : '';
    }

    public get selectedAppState() {
        return this.snapshot.selectedApp;
    }

    public apps =
        this.project(s => s.apps);

    public selectedApp =
        this.project(s => s.selectedApp);

    public selectedValidApp =
        this.selectedApp.pipe(filter(x => !!x), map(x => <AppDto>x),
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

    public create(request: CreateAppDto): Observable<AppDto> {
        return this.appsService.postApp(request).pipe(
            tap(created => {
                this.next(s => {
                    const apps = s.apps.push(created).sortByStringAsc(x => x.displayName);

                    return { ...s, apps };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public update(app: AppDto, request: UpdateAppDto): Observable<AppDto> {
        return this.appsService.putApp(app, request, app.version).pipe(
            tap(updated => {
                this.next(s => {
                    const apps = s.apps.replaceBy('id', updated);

                    const selectedApp =
                        s.selectedApp &&
                        s.selectedApp.id === app.id ?
                        updated :
                        s.selectedApp;

                    return { ...s, apps, selectedApp };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));

    }

    public delete(app: AppDto): Observable<any> {
        return this.appsService.deleteApp(app).pipe(
            tap(() => {
                this.next(s => {
                    const apps = s.apps.filter(x => x.name !== app.name);

                    const selectedApp =
                        s.selectedApp &&
                        s.selectedApp.id === app.id ?
                        null :
                        s.selectedApp;

                    return { ...s, apps, selectedApp };
                });
            }),
            shareSubscribed(this.dialogs));
    }
}
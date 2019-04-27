/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { distinctUntilChanged, map, share } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    State
} from '@app/framework';

import {
    AppDto,
    AppsService,
    CreateAppDto
} from './../services/apps.service';

interface Snapshot {
    // All apps, loaded once.
    apps: AppsList;

    // The selected app.
    selectedApp: AppDto | null;
}

type AppsList = ImmutableArray<AppDto>;

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
        const http$ =
            this.loadApp(name)
                .pipe(share());

        http$.subscribe(selectedApp => {
            this.next(s => ({ ...s, selectedApp }));
        });

        return http$;
    }

    private loadApp(name: string | null) {
        return of(name ? this.snapshot.apps.find(x => x.name === name) || null : null);
    }

    public load(): Observable<any> {
        const http$ =
            this.appsService.getApps().pipe(
                share());

        http$.subscribe(response => {
            this.next(s => {
                const apps = ImmutableArray.of(response).sortByStringAsc(x => x.name);

                return { ...s, apps };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return http$;
    }

    public create(request: CreateAppDto): Observable<AppDto> {
        const http$ =
            this.appsService.postApp(request).pipe(
                share());

        http$.subscribe(app => {
            this.next(s => {
                const apps = s.apps.push(app).sortByStringAsc(x => x.name);

                return { ...s, apps };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return http$;
    }

    public delete(name: string): Observable<any> {
        const http$ =
            this.appsService.deleteApp(name).pipe(
                share());

        http$.subscribe(() => {
            this.next(s => {
                const apps = s.apps.filter(x => x.name !== name);

                const selectedApp =
                    s.selectedApp &&
                    s.selectedApp.name === name ?
                    null :
                    s.selectedApp;

                return { ...s, apps, selectedApp };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return http$;
    }
}
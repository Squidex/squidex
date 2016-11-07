/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    BehaviorSubject,
    Observable,
    Subject
} from 'rxjs';

import {
    AppCreateDto,
    AppDto,
    AppsService
} from './apps.service';

import { AuthService } from './auth.service';

@Ng2.Injectable()
export class AppsStoreService {
    private lastApps: AppDto[] | null = null;
    private readonly apps$ = new Subject<AppDto[]>();
    private readonly appName$ = new BehaviorSubject<string | null>(null);

    private readonly appsPublished$ =
        this.apps$
            .distinctUntilChanged()
            .publishReplay(1)
            .refCount();

    private readonly selectedApp$ =
        this.appsPublished$.combineLatest(this.appName$, (apps, name) => apps && name ? apps.find(x => x.name === name) || null : null)
            .distinctUntilChanged()
            .publishReplay(1)
            .refCount();

    public get apps(): Observable<AppDto[]> {
        return this.appsPublished$;
    }

    public get selectedApp(): Observable<AppDto | null> {
        return this.selectedApp$;
    }

    constructor(
        private readonly auth: AuthService,
        private readonly appService: AppsService
    ) {
        if (!auth || !appService) {
            return;
        }

        this.appsPublished$.subscribe(apps => {
            this.lastApps = apps;
        });

        this.auth.isAuthenticatedChanges.subscribe(isAuthenticated => {
            if (isAuthenticated) {
                this.load();
            }
        });
    }

    public reload() {
        if (this.auth.isAuthenticated) {
            this.load();
        }
    }

    private load() {
        this.appService.getApps().subscribe(apps => {
            this.apps$.next(apps);
        });
    }

    public selectApp(name: string | null): Observable<AppDto | null> {
        this.appName$.next(name);

        return this.selectedApp;
    }

    public createApp(appToCreate: AppCreateDto): Observable<AppDto> {
        return this.appService.postApp(appToCreate).do(app => {
            if (this.lastApps && app) {
                this.apps$.next(this.lastApps.concat([app]));
            }
        });
    }
}
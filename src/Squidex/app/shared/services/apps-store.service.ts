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
    AppDto,
    AppsService,
    CreateAppDto
} from './apps.service';

import { DateTime } from 'framework';

import { AuthService } from './auth.service';

@Ng2.Injectable()
export class AppsStoreService {
    private readonly apps$ = new Subject<AppDto[]>();
    private readonly appName$ = new BehaviorSubject<string | null>(null);
    private lastApps: AppDto[] | null = null;
    private isAuthenticated = false;

    private readonly appsPublished$ =
        this.apps$
            .distinctUntilChanged()
            .publishReplay(1);

    private readonly selectedApp$ =
        this.appsPublished$.combineLatest(this.appName$, (apps, name) => apps && name ? apps.find(x => x.name === name) || null : null)
            .distinctUntilChanged()
            .publishReplay(1);

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

        this.selectedApp$.connect();

        this.appsPublished$.connect();
        this.appsPublished$.subscribe(apps => {
            this.lastApps = apps;
        });

        this.auth.isAuthenticated.subscribe(isAuthenticated => {
            this.isAuthenticated = isAuthenticated;

            if (isAuthenticated) {
                this.load();
            }
        });
    }

    public reload() {
        if (this.isAuthenticated) {
            this.load();
        }
    }

    private load() {
        this.appService.getApps().subscribe(apps => {
            this.apps$.next(apps);
        });
    }

    public selectApp(name: string | null): Promise<boolean> {
        this.appName$.next(name);

        return this.selectedApp.take(1).map(app => app !== null).toPromise();
    }

    public createApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appService.postApp(dto)
            .map(created => {
                now = now || DateTime.now();

                const app = new AppDto(created.id, dto.name, now, now, 'Owner');

                return app;
            })
            .do(app => {
                if (this.lastApps && app) {
                    this.apps$.next(this.lastApps.concat([app]));
                }
            });
    }
}
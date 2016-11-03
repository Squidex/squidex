/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { BehaviorSubject, Observable } from 'rxjs';

import { 
    AppCreateDto, 
    AppDto, 
    AppsService
} from './apps.service';

import { AuthService } from './auth.service';

@Ng2.Injectable()
export class AppsStoreService {
    private lastApps: AppDto[] = null;
    private readonly apps$ = new BehaviorSubject<AppDto[]>(null);

    public get apps(): Observable<AppDto[]> {
        return this.apps$;
    }

    constructor(
        private readonly authService: AuthService,
        private readonly appService: AppsService
    ) {
        if (!authService || !appService) {
            return;
        }

        this.apps$.subscribe(apps => {
            this.lastApps = apps;
        });

        this.authService.isAuthenticatedChanges.subscribe(isAuthenticated => {
            if (isAuthenticated) {
                this.load();
            }
        });
    }

    public reload() {
        if (this.authService.isAuthenticated) {
            this.load();
        }
    }

    private load() {
        this.appService.getApps().subscribe(apps => {
            this.apps$.next(apps);
        });
    }

    public createApp(appToCreate: AppCreateDto): Observable<any> {
        return this.appService.postApp(appToCreate).do(app => {
            if (this.lastApps && app) {
                this.apps$.next(this.lastApps.concat([app]));
            }
        }); 
    }
}
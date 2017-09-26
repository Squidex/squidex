/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Observer, ReplaySubject } from 'rxjs';

import { DateTime } from 'framework';

import {
    AppDto,
    AppsService,
    CreateAppDto
} from './apps.service';

@Injectable()
export class AppsStoreService {
    private readonly apps$ = new ReplaySubject<AppDto[]>(1);
    private readonly app$ = new BehaviorSubject<AppDto | null>(null);

    public get apps(): Observable<AppDto[]> {
        return this.apps$;
    }

    public get selectedApp(): Observable<AppDto | null> {
        return this.app$;
    }

    constructor(
        private readonly appsService: AppsService
    ) {
        if (!appsService) {
            return;
        }

       this.appsService.getApps()
            .subscribe(apps => {
                this.apps$.next(apps);
            }, error => {
                this.apps$.next([]);
            });
    }

    public selectApp(name: string | null): Observable<boolean> {
        return Observable.create((observer: Observer<boolean>) => {
            this.apps$.subscribe(apps => {
                const app = apps.find(x => x.name === name) || null;

                this.app$.next(app);

                observer.next(app !== null);
                observer.complete();
            }, error => {
                observer.error(error);
            });
        });
    }

    public createApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appsService.postApp(dto)
            .do(app => {
                this.apps$.first().subscribe(apps => {
                    this.apps$.next(apps.concat([app]));
                });
            });
    }
}
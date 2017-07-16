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

import { AuthService } from './auth.service';

@Injectable()
export class AppsStoreService {
    private readonly apps$ = new ReplaySubject<AppDto[]>();
    private readonly app$ = new BehaviorSubject<AppDto | null>(null);

    public get apps(): Observable<AppDto[]> {
        return this.apps$;
    }

    public get selectedApp(): Observable<AppDto | null> {
        return this.app$;
    }

    constructor(
        private readonly authService: AuthService,
        private readonly appsService: AppsService
    ) {
        if (!appsService) {
            return;
        }

        this.authService.isAuthenticated.filter(t => !!t).first()
            .switchMap(() => this.appsService.getApps())
            .subscribe(apps => {
                this.apps$.next(apps);
            });
    }

    public selectApp(name: string | null): Promise<boolean> {
        return Observable.create((observer: Observer<boolean>) => {
            this.apps$.subscribe(apps => {
                const app = apps.find(x => x.name === name) || null;

                this.app$.next(app);

                observer.next(app !== null);
                observer.complete();
            }, error => {
                observer.error(error);
            });
        }).toPromise();
    }

    public createApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appsService.postApp(dto)
            .map(created => {
                now = now || DateTime.now();

                return new AppDto(created.id, dto.name, 'Owner', now, now);
            })
            .do(app => {
                this.apps$.defaultIfEmpty().first().subscribe(apps => {
                    if (apps) {
                        this.apps$.next(apps.concat([app]));
                    }
                });
            });
    }
}
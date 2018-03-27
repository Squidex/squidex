
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import 'framework/utils/rxjs-extensions';

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

import { DateTime, ImmutableArray } from 'framework';

import {
    AppDto,
    AppsService,
    CreateAppDto
} from './../services/apps.service';

@Injectable()
export class AppsState {
    public apps = new BehaviorSubject<ImmutableArray<AppDto>>(ImmutableArray.empty());

    public selectedApp = new BehaviorSubject<AppDto | null>(null);

    constructor(
        private readonly appsService: AppsService
    ) {
    }

    public loadApps(): Observable<any> {
        return this.appsService.getApps()
            .do(dtos => {
                this.apps.nextBy(v => ImmutableArray.of(dtos));
            });
    }

    public selectApp(name: string | null): Observable<AppDto | null> {
        const observable =
            !name ?
            Observable.of(null) :
            Observable.of(this.apps.value.find(x => x.name === name) || null);

        return observable
            .do(app => {
                this.selectedApp.next(app);
            });
    }

    public createApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appsService.postApp(dto)
            .do(app => {
                this.apps.nextBy(v => v.push(app));
            });
    }

    public deleteApp(name: string): Observable<any> {
        return this.appsService.deleteApp(name)
            .do(app => {
                this.apps.nextBy(v => v.filter(a => a.name !== name));
            });
    }
}
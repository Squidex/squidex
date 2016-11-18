/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import { Observable } from 'rxjs';

import { ApiUrlConfig, DateTime } from 'framework';

import { AuthService } from './auth.service';

export class AppDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime
    ) {
    }
}

export class AppCreateDto {
    constructor(
        public readonly name: string
    ) {
    }
}

@Ng2.Injectable()
export class AppsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getApps(): Observable<AppDto[]> {
        return this.authService.authGet(this.apiUrl.buildUrl('/api/apps'))
                .map(response => {
                    const body: any[] = response.json() || [];

                    return body.map(item => {
                        return new AppDto(
                            item.id,
                            item.name,
                            DateTime.parseISO(item.created),
                            DateTime.parseISO(item.lastModified)
                        );
                    });
                });
    }

    public postApp(appToCreate: AppCreateDto, now?: DateTime): Observable<AppDto> {
        now = now || DateTime.now();

        return this.authService.authPost(this.apiUrl.buildUrl('api/apps'), appToCreate)
                .map(response => response.json())
                .map(response => new AppDto(response.id, appToCreate.name, now, now))
                .catch(response => {
                    if (response.status === 400) {
                        return Observable.throw('An app with the same name already exists.');
                    } else {
                        return Observable.throw('A new app could not be created.');
                    }
                });
    }
}
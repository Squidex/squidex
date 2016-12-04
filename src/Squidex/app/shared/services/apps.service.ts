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

import { handleError } from './errors';

export class AppDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly permission: string
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
        const url = this.apiUrl.buildUrl('/api/apps');

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppDto(
                            item.id,
                            item.name,
                            DateTime.parseISO(item.created),
                            DateTime.parseISO(item.lastModified),
                            item.permission
                        );
                    });
                })
                .catch(response => handleError('Failed to load apps. Please reload.', response));
    }

    public postApp(app: AppCreateDto, now?: DateTime): Observable<AppDto> {
        now = now || DateTime.now();

        const url = this.apiUrl.buildUrl('api/apps');

        return this.authService.authPost(url, app)
                .map(response => response.json())
                .map(response => {
                    return new AppDto(
                        response.id, 
                        app.name, 
                        now, 
                        now, 
                        'Owner');
                })
                .catch(response => handleError('Failed to create app. Please reload.', response));
    }
}
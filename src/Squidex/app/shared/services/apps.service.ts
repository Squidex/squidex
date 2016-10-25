/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable } from 'rxjs';

import { ApiUrlConfig, DateTime } from './../../framework';
import { AuthService } from './auth.service';

export class AppDto {
    constructor(
        private readonly id: string,
        private readonly name: string,
        private readonly created: DateTime,
        private readonly lastModified: DateTime
    ) {
    }
}

export class AppCreateDto {
    constructor(
        private readonly name: string
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
                    let body: any[] = response.json() || [];

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

    public postApp(app: AppCreateDto): Observable<any> {
        return this.authService.authPost(this.apiUrl.buildUrl('api/apps'), app);
    }
}
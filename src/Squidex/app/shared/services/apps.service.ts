/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    DateTime,
    EntityCreatedDto
} from 'framework';

import { AuthService } from './auth.service';

export class AppDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly permission: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime
    ) {
    }
}

export class CreateAppDto {
    constructor(
        public readonly name: string
    ) {
    }
}

@Injectable()
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
                            item.permission,
                            DateTime.parseISO(item.created),
                            DateTime.parseISO(item.lastModified));
                    });
                })
                .catchError('Failed to load apps. Please reload.');
    }

    public postApp(dto: CreateAppDto): Observable<EntityCreatedDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catchError('Failed to create app. Please reload.');
    }
}
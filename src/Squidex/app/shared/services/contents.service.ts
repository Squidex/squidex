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
    Version
} from 'framework';

import { AuthService } from './auth.service';

export class ContentsDto {
    constructor(
        public readonly total: number,
        public readonly items: ContentDto[]
    ) {
    }
}

export class ContentDto {
    constructor(
        public readonly id: string,
        public readonly isPublished: boolean,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly data: any,
        public readonly version: Version
    ) {
    }
}

@Injectable()
export class ContentsService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getContents(appName: string, schemaName: string, take: number, skip: number, query: string): Observable<ContentsDto> {
        let fullQuery = query ? query.trim() : '';

        if (fullQuery.length > 0) {
            if (fullQuery.indexOf('$filter') < 0 &&
                fullQuery.indexOf('$search') < 0 &&
                fullQuery.indexOf('$orderby') < 0) {
                fullQuery = `&$search=${fullQuery}`;
            } else {
                fullQuery = `&${fullQuery}`;
            }
        }

        if (take > 0) {
            fullQuery += `&$top=${take}`;
        }

        if (skip > 0) {
            fullQuery += `&$skip=${skip}`;
        }

        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?nonPublished=true&hidden=true${fullQuery}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response.items;

                    return new ContentsDto(response.total, items.map(item => {
                        return new ContentDto(
                            item.id,
                            item.isPublished,
                            item.createdBy,
                            item.lastModifiedBy,
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified),
                            item.data,
                            new Version(item.version.toString()));
                    }));
                })
                .catchError('Failed to load contents. Please reload.');
    }

    public getContent(appName: string, schemaName: string, id: string, version: Version): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}?hidden=true`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
                .map(response => {
                    return new ContentDto(
                        response.id,
                        response.isPublished,
                        response.createdBy,
                        response.lastModifiedBy,
                        DateTime.parseISO_UTC(response.created),
                        DateTime.parseISO_UTC(response.lastModified),
                        response.data,
                        new Version(response.version.toString()));
                })
                .catchError('Failed to load content. Please reload.');
    }

    public postContent(appName: string, schemaName: string, dto: any, publish: boolean, version: Version): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?publish=${publish}`);

        return this.authService.authPost(url, dto, version)
                .map(response => response.json())
                .map(response => {
                    return new ContentDto(
                        response.id,
                        response.isPublished,
                        response.createdBy,
                        response.lastModifiedBy,
                        DateTime.parseISO_UTC(response.created),
                        DateTime.parseISO_UTC(response.lastModified),
                        response.data,
                        new Version(response.version.toString()));
                })
                .catchError('Failed to create content. Please reload.');
    }

    public putContent(appName: string, schemaName: string, id: string, dto: any, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return this.authService.authPut(url, dto, version)
                .catchError('Failed to update content. Please reload.');
    }

    public publishContent(appName: string, schemaName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/publish`);

        return this.authService.authPut(url, {}, version)
                .catchError('Failed to publish content. Please reload.');
    }

    public unpublishContent(appName: string, schemaName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}/unpublish`);

        return this.authService.authPut(url, {}, version)
                .catchError('Failed to unpublish content. Please reload.');
    }

    public deleteContent(appName: string, schemaName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return this.authService.authDelete(url, version)
                .catchError('Failed to delete content. Please reload.');
    }
}
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
        public readonly data: any
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
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}?take=${take}&skip=${skip}&query=${query}&nonPublished=true&hidden=true`);

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
                            item.data);
                    }));
                })
                .catchError('Failed to load contents. Please reload.');
    }

    public getContent(appName: string, schemaName: string, id: string): Observable<ContentDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}?hidden=true`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    return new ContentDto(
                        response.id,
                        response.isPublished,
                        response.createdBy,
                        response.lastModifiedBy,
                        DateTime.parseISO_UTC(response.created),
                        DateTime.parseISO_UTC(response.lastModified),
                        response.data);
                })
                .catchError('Failed to load content. Please reload.');
    }

    public postContent(appName: string, schemaName: string, dto: any): Observable<EntityCreatedDto> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catchError('Failed to create content. Please reload.');
    }

    public putContent(appName: string, schemaName: string, id: string, dto: any): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return this.authService.authPut(url, dto)
                .catchError('Failed to update Content. Please reload.');
    }

    public deleteContent(appName: string, schemaName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`/api/content/${appName}/${schemaName}/${id}`);

        return this.authService.authDelete(url)
                .catchError('Failed to delete Content. Please reload.');
    }
}
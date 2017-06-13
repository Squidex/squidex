/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, Version } from 'framework';
import { AuthService } from './auth.service';

export class WebhookDto {
    constructor(
        public readonly id: string,
        public readonly schemaId: string,
        public readonly sharedSecret: string,
        public readonly url: string
    ) {
    }
}

export class CreateWebhookDto {
    constructor(
        public readonly url: string
    ) {
    }
}

@Injectable()
export class WebhooksService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getWebhooks(appName: string, version?: Version): Observable<WebhookDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new WebhookDto(
                            item.id,
                            item.schemaId,
                            item.sharedSecret,
                            item.url);
                    });
                })
                .catchError('Failed to load webhooks. Please reload.');
    }

    public postWebhook(appName: string, schemaName: string, dto: CreateWebhookDto, version?: Version): Observable<WebhookDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/webhooks`);

        return this.authService.authPost(url, dto, version)
                .map(response => response.json())
                .map(response => {
                    return new WebhookDto(
                        response.id,
                        response.schemaId,
                        response.sharedSecret,
                        response.url);
                })
                .catchError('Failed to create webhook. Please reload.');
    }

    public deleteWebhook(appName: string, schemaName: string, id: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/webhooks/${id}`);

        return this.authService.authDelete(url, version)
                .catchError('Failed to delete webhook. Please reload.');
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs';

import { ApiUrlConfig, EntityCreatedDto } from 'framework';
import { AuthService } from './auth.service';

@Injectable()
export class AssetsService {
    constructor(
        private readonly http: Http,
        private readonly apiUrl: ApiUrlConfig,
        private readonly authService: AuthService
    ) {
    }

    public uploadFile(appName: string, file: File): Observable<number | EntityCreatedDto> {
        return new Observable<number | EntityCreatedDto>(subscriber => {
            const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

            const content = new FormData();
            const headers = new Headers({
                'Authorization': `${this.authService.user.user.token_type} ${this.authService.user.user.access_token}`,
                'Content-Type': 'multipart/form-data'
            });

            content.append('file', file);

            this.http
                .post(url, content, headers)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catchError('Failed to upload asset. Please reload.')
                .subscribe(value => {
                    subscriber.next(value);
                }, err => {
                    subscriber.error(err);
                }, () => {
                    subscriber.complete();
                });
        });
    }
}
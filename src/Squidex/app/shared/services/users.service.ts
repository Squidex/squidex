/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
    ApiUrlConfig,
    HTTP,
    pretifyError
} from '@app/framework';

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly displayName: string
    ) {
    }
}

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(query?: string): Observable<UserDto[]> {
        const url = this.apiUrl.buildUrl(`api/users?query=${query || ''}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    const items: any[] = body;

                    return items.map(item => {
                        return new UserDto(
                            item.id,
                            item.displayName);
                    });
                }),
                pretifyError('Failed to load users. Please reload.'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    return new UserDto(
                        body.id,
                        body.displayName);
                }),
                pretifyError('Failed to load user. Please reload.'));
    }
}
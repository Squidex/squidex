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
import { ApiUrlConfig, pretifyError, Resource, StringHelper } from '@app/framework';

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly displayName: string,
    ) {
    }
}

export type UpdateProfileDto = Readonly<{
    // The given answers.
    answers: { [question: string]: string };
 }>;

@Injectable({
    providedIn: 'root',
})
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public postUser(dto: UpdateProfileDto): Observable<any> {
        const url = this.apiUrl.buildUrl('api/user');

        return this.http.post<any[]>(url, dto);
    }

    public getUsers(query?: string): Observable<ReadonlyArray<UserDto>> {
        const url = this.apiUrl.buildUrl(`api/users${StringHelper.buildQuery({ query })}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return parseUsers(body);
            }),
            pretifyError('i18n:users.loadFailed'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.loadUserFailed'));
    }

    public getResources(): Observable<Resource> {
        const url = this.apiUrl.buildUrl('api');

        return this.http.get<Resource>(url).pipe(
            pretifyError('i18n:users.loadUserFailed'));
    }
}

function parseUsers(response: any[]) {
    return response.map(parseUser);
}

function parseUser(response: any) {
    return new UserDto(
        response.id,
        response.displayName);
}


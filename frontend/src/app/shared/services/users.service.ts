/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, pretifyError, Resource, StringHelper } from '@app/framework';
import { IUpdateProfileDto, UpdateProfileDto, UserDto } from './../model';

@Service()
export class UsersService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public postUser(dto: IUpdateProfileDto): Observable<any> {
        const url = this.apiUrl.buildUrl('api/user');

        return this.http.post<any[]>(url, new UpdateProfileDto(dto));
    }

    public getUsers(query?: string): Observable<ReadonlyArray<UserDto>> {
        const url = this.apiUrl.buildUrl(`api/users${StringHelper.buildQuery({ query })}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(UserDto.fromJSON);
            }),
            pretifyError('i18n:users.loadFailed'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return UserDto.fromJSON(body);
            }),
            pretifyError('i18n:users.loadUserFailed'));
    }

    public getResources(): Observable<Resource> {
        const url = this.apiUrl.buildUrl('api');

        return this.http.get<Resource>(url).pipe(
            pretifyError('i18n:users.loadUserFailed'));
    }
}
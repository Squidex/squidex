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
import { ApiUrlConfig, CreateUserDto, IResourceDto, pretifyError, StringHelper, UpdateUserDto, UserDto, UsersDto } from '@app/shared';
export { UserDto, UsersDto };

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getUsers(take: number, skip: number, query?: string): Observable<UsersDto> {
        const url = this.apiUrl.buildUrl(`api/user-management${StringHelper.buildQuery({ take, skip, query })}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return UsersDto.fromJSON(body);
            }),
            pretifyError('i18n:users.loadFailed'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.http.get(url).pipe(
            map(body => {
                return UserDto.fromJSON(body);
            }),
            pretifyError('i18n:users.loadUserFailed'));
    }

    public postUser(dto: CreateUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl('api/user-management');

        return this.http.post(url, dto.toJSON()).pipe(
            map(body => {
                return UserDto.fromJSON(body);
            }),
            pretifyError('i18n:users.createFailed'));
    }

    public putUser(user: IResourceDto, dto: UpdateUserDto): Observable<UserDto> {
        const link = user._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, { body: dto.toJSON() }).pipe(
            map(body => {
                return UserDto.fromJSON(body);
            }),
            pretifyError('i18n:users.updateFailed'));
    }

    public lockUser(user: IResourceDto): Observable<UserDto> {
        const link = user._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return UserDto.fromJSON(body);
            }),
            pretifyError('i18n:users.lockFailed'));
    }

    public unlockUser(user: IResourceDto): Observable<UserDto> {
        const link = user._links['unlock'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return UserDto.fromJSON(body);
            }),
            pretifyError('i18n:users.unlockFailed'));
    }

    public deleteUser(user: IResourceDto): Observable<any> {
        const link = user._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:users.deleteFailed'));
    }
}
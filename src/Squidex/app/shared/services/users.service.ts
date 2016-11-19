/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable } from 'rxjs';

import { ApiUrlConfig } from 'framework';

import { AuthService } from './auth.service';

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly pictureUrl: string
    ) {
    }
}

@Ng2.Injectable()
export class UsersService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(query?: string): Observable<UserDto[]> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/users/?query=${query || ''}`))
                .map(response => {                    
                    const body: any[] = response.json() || [];

                    return body.map(item => {
                        return new UserDto(
                            item.id,
                            item.email,
                            item.displayName,
                            item.pictureUrl);
                    });
                });
    }

    public getUser(id: string): Observable<UserDto> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/users/${id}`))
                .map(response => {                    
                    const body: any = response.json();

                    return new UserDto(
                        body.id,
                        body.email,
                        body.displayName,
                        body.pictureUrl);
                });
    }
}
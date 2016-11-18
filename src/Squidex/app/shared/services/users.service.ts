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

export class User {
    constructor(
        public readonly id: string,
        public readonly profileUrl: string,
        public readonly displayName: string
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

    public getUsers(query?: string): Observable<User[]> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/users/?query=${query || ''}`))
                .map(response => {                    
                    const body: any[] = response.json() || [];

                    return body.map(item => {
                        return new User(
                            item.id,
                            item.profileUrl,
                            item.displayName);
                    });
                });
    }

    public getUser(id: string): Observable<User> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/users/${id}`))
                .map(response => {                    
                    const body: any = response.json();

                    return new User(
                        body.id,
                        body.profileUrl,
                        body.displayName);
                });
    }
}
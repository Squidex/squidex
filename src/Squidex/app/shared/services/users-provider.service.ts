/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable,  } from 'rxjs';

import { User, UsersService } from './users.service';

import { AuthService } from './auth.service';

@Ng2.Injectable()
export class UsersProviderService {
    private readonly caches: { [id: string]: Observable<User> } = {};

    constructor(
        private readonly usersService: UsersService,
        private readonly authService: AuthService,
    ) {
    }

    public getUser(id: string): Observable<User> {
        let result = this.caches[id];

        if (!result) {
            result = this.caches[id] = 
                this.usersService.getUser(id)
                    .map(u => {
                        if (this.authService.user && u.id === this.authService.user.id) {
                            return new User(u.id, u.profileUrl, 'Me');
                        } else {
                            return u;
                        }
                    })
                    .publishLast().refCount();
        }

        return result;
    }
}
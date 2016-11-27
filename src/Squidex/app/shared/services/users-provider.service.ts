/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable,  } from 'rxjs';

import { UserDto, UsersService } from './users.service';

import { AuthService } from './auth.service';

@Ng2.Injectable()
export class UsersProviderService {
    private readonly caches: { [id: string]: Observable<UserDto> } = {};

    constructor(
        private readonly usersService: UsersService,
        private readonly authService: AuthService,
    ) {
    }

    public getUser(id: string): Observable<UserDto> {
        let result = this.caches[id];

        if (!result) {
            const request = 
                this.usersService.getUser(id).retry(2)
                    .map(u => {
                        if (this.authService.user && u.id === this.authService.user.id) {
                            return new UserDto(u.id, u.email, 'Me', u.pictureUrl);
                        } else {
                            return u;
                        }
                    })
                    .publishLast();

            request.connect();

            result = this.caches[id] = request;
        }

        return result;
    }
}
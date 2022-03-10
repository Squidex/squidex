/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { catchError, map, Observable, of, share, shareReplay } from 'rxjs';
import { AuthService } from './auth.service';
import { UserDto, UsersService } from './users.service';

@Injectable()
export class UsersProviderService {
    private readonly caches: { [id: string]: Observable<UserDto> } = {};

    constructor(
        private readonly usersService: UsersService,
        private readonly authService: AuthService,
    ) {
    }

    public getUser(id: string, me: string | null = 'Me'): Observable<UserDto> {
        let result = this.caches[id];

        if (!result) {
            result =
                this.usersService.getUser(id).pipe(
                    catchError(() => {
                        return of(new UserDto('Unknown', 'Unknown'));
                    }),
                    shareReplay(1));

            this.caches[id] = result;
        }

        return result.pipe(
            map(dto => {
                if (me && this.authService.user && dto.id === this.authService.user.id) {
                    dto = new UserDto(dto.id, me);
                }
                return dto;
            }),
            share());
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { catchError, map, Observable, of, share, shareReplay } from 'rxjs';
import { UserDto } from '../model';
import { AuthService } from './auth.service';
import { UsersService } from './users.service';

@Injectable({
    providedIn: 'root',
})
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
                        return of(new UserDto({ id: 'Unknown', displayName: 'Unknown' } as any));
                    }),
                    shareReplay(1));

            this.caches[id] = result;
        }

        return result.pipe(
            map(dto => {
                if (me && this.authService.user && dto.id === this.authService.user.id) {
                    dto = new UserDto({ id: dto.id, displayName: me } as any);
                }
                return dto;
            }),
            share());
    }
}

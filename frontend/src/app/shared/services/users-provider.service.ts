/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ConnectableObservable, Observable, of } from 'rxjs';
import { catchError, map, publishLast, share } from 'rxjs/operators';
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
            const request =
                this.usersService.getUser(id).pipe(
                    catchError(() => {
                        return of(new UserDto('Unknown', 'Unknown'));
                    }),
                    publishLast());

            (<ConnectableObservable<any>>request).connect();

            // eslint-disable-next-line no-multi-assign
            result = this.caches[id] = request;
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

/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { UserDto, UsersService } from './users.service';

import { AuthService } from './auth.service';

@Injectable()
export class UsersProviderService {
    private readonly caches: { [id: string]: Observable<UserDto> } = {};

    constructor(
        private readonly usersService: UsersService,
        private readonly authService: AuthService
    ) {
    }

    public getUser(id: string): Observable<UserDto> {
        let result = this.caches[id];

        if (!result) {
            const request =
                this.usersService.getUser(id).retry(2)
                    .catch(err => {
                        return Observable.of(new UserDto('NOT FOUND', 'NOT FOUND', 'NOT FOUND', ''));
                    })
                    .map(dto => {
                        if (this.authService.user && dto.id === this.authService.user.id) {
                            dto = new UserDto(dto.id, dto.email, 'Me', dto.pictureUrl);
                        }
                        return dto;
                    })
                    .publishLast();

            request.connect();

            result = this.caches[id] = request;
        }

        return result;
    }
}
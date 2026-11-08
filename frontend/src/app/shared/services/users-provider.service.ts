/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, inject, Service, Signal, signal, untracked } from '@angular/core';
import { catchError, of } from 'rxjs';
import { UserDto } from '../model';
import { AuthService } from './auth.service';
import { UsersService } from './users.service';

@Service()
export class UsersProviderService {
    private readonly authService = inject(AuthService);
    private readonly usersService = inject(UsersService);
    private readonly loaded: { [id: string]: Signal<UserDto | undefined> } = {};
    private readonly mapped: { [key: string]: Signal<UserDto | undefined> } = {};

    public user(id: string, me: string | null = 'Me'): Signal<UserDto | undefined> {
        const key = `${id}|${me}`;

        let result = this.mapped[key];
        if (!result) {
            const source = this.load(id);

            result = computed(() => {
                const dto = source();
                if (!dto) {
                    return undefined;
                }

                const currentUser = this.authService.user;
                if (me && currentUser && dto.id === currentUser.id) {
                    return new UserDto({ id: dto.id, displayName: me } as any);
                }

                return dto;
            });

            this.mapped[key] = result;
        }

        return result;
    }

    private load(id: string) {
        let result = this.loaded[id];
        if (!result) {
            const user = signal<UserDto | undefined>(undefined);

            // Pipes call this while the template is rendered, so the request must not be tracked.
            untracked(() => {
                this.usersService.getUser(id).pipe(
                    catchError(() => {
                        return of(new UserDto({ id: 'Unknown', displayName: 'Unknown' } as any));
                    }))
                .subscribe(dto => user.set(dto));
            });

            result = user.asReadonly();

            this.loaded[id] = result;
        }

        return result;
    }
}

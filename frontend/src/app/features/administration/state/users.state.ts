/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable object-curly-newline */

import { Injectable } from '@angular/core';
import '@app/framework/utils/rxjs-extensions';
import { EMPTY, Observable, of } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { DialogService, getPagingInfo, ListState, shareSubscribed, State } from '@app/shared';
import { UpsertUserDto, UserDto, UsersService } from './../services/users.service';

interface Snapshot extends ListState<string> {
    // The current users.
    users: ReadonlyArray<UserDto>;

    // The selected user.
    selectedUser?: UserDto | null;

    // Indicates if the user can create a user.
    canCreate?: boolean;
}

@Injectable()
export class UsersState extends State<Snapshot> {
    public users =
        this.project(x => x.users);

    public paging =
        this.project(x => getPagingInfo(x, x.users.length));

    public query =
        this.project(x => x.query);

    public selectedUser =
        this.project(x => x.selectedUser);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    constructor(
        private readonly dialogs: DialogService,
        private readonly usersService: UsersService,
    ) {
        super({
            users: [],
            page: 0,
            pageSize: 10,
            total: 0,
        }, 'Users');
    }

    public select(id: string | null): Observable<UserDto | null> {
        return this.loadUser(id).pipe(
            tap(selectedUser => {
                this.next({ selectedUser }, 'Selected');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    private loadUser(id: string | null) {
        if (!id) {
            return of(null);
        }

        const found = this.snapshot.users.find(x => x.id === id);

        if (found) {
            return of(found);
        }

        return this.usersService.getUser(id).pipe(
            catchError(() => of(null)));
    }

    public load(isReload = false, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            const { selectedUser } = this.snapshot;

            this.resetState({ selectedUser, ...update }, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        const { page, pageSize, query } = this.snapshot;

        return this.usersService.getUsers(
                pageSize,
                pageSize * page,
                query).pipe(
            tap(({ total, items: users, canCreate }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:users.reloaded');
                }

                this.next(s => {
                    let selectedUser = s.selectedUser;

                    if (selectedUser) {
                        selectedUser = users.find(x => x.id === selectedUser!.id) || selectedUser;
                    }

                    return { ...s,
                        canCreate,
                        users,
                        isLoaded: true,
                        isLoading: false,
                        selectedUser,
                        total,
                    };
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: UpsertUserDto): Observable<UserDto> {
        return this.usersService.postUser(request).pipe(
            tap(created => {
                this.next(s => {
                    const users = [created, ...s.users].slice(0, s.pageSize);

                    return { ...s, users, total: s.total + 1 };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public update(user: UserDto, request: Partial<UpsertUserDto>): Observable<UserDto> {
        return this.usersService.putUser(user, request).pipe(
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public lock(user: UserDto): Observable<UserDto> {
        return this.usersService.lockUser(user).pipe(
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public unlock(user: UserDto): Observable<UserDto> {
        return this.usersService.unlockUser(user).pipe(
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(user: UserDto) {
        return this.usersService.deleteUser(user).pipe(
            tap(() => {
                this.next(s => {
                    const users = s.users.removedBy('id', user);

                    const selectedUser =
                        s.selectedUser?.id !== user.id ?
                        s.selectedUser :
                        null;

                    return { ...s, users, total: s.total - 1, selectedUser };
                }, 'Delete');
            }),
            shareSubscribed(this.dialogs));
    }

    public search(query: string) {
        if (!this.next({ query, page: 0, total: 0 }, 'Loading Search')) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    public page(paging: { page: number; pageSize: number }) {
        if (!this.next(paging, 'Loading Paged')) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    private replaceUser(user: UserDto) {
        return this.next(s => {
            const users = s.users.replacedBy('id', user);

            const selectedUser =
                s.selectedUser?.id !== user.id ?
                s.selectedUser :
                user;

            return { ...s, users, selectedUser };
        }, 'Updated');
    }
}

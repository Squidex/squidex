/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import '@app/framework/utils/rxjs-extensions';
import { DialogService, Pager, shareSubscribed, State, StateSynchronizer } from '@app/shared';
import { Observable, of } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { CreateUserDto, UpdateUserDto, UserDto, UsersService } from './../services/users.service';

interface Snapshot {
    // The current users.
    users: UsersList;

    // The pagination information.
    usersPager: Pager;

    // The query to filter users.
    usersQuery?: string;

    // Indicates if the users are loaded.
    isLoaded?: boolean;

    // Indicates if the users are loading.
    isLoading?: boolean;

    // The selected user.
    selectedUser?: UserDto | null;

    // Indicates if the user can create a user.
    canCreate?: boolean;
}

export type UsersList = ReadonlyArray<UserDto>;
export type UsersResult = { total: number, users: UsersList };

@Injectable()
export class UsersState extends State<Snapshot> {
    public users =
        this.project(x => x.users);

    public usersPager =
        this.project(x => x.usersPager);

    public usersQuery =
        this.project(x => x.usersQuery);

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
        private readonly usersService: UsersService
    ) {
        super({
            users: [],
            usersPager: new Pager(0)
        });
    }

    public select(id: string | null): Observable<UserDto | null> {
        return this.loadUser(id).pipe(
            tap(selectedUser => {
                this.next(s => ({ ...s, selectedUser }));
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

        return this.usersService.getUser(id).pipe(catchError(() => of(null)));
    }

    public loadAndListen(synchronizer: StateSynchronizer) {
        synchronizer.mapTo(this)
            .keep('selectedUser')
            .withPager('usersPager', 'users', 10)
            .withString('usersQuery', 'q')
            .whenSynced(() => this.loadInternal(false))
            .build();
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState({ selectedUser: this.snapshot.selectedUser });
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true });

        return this.usersService.getUsers(
                this.snapshot.usersPager.pageSize,
                this.snapshot.usersPager.skip,
                this.snapshot.usersQuery).pipe(
            tap(({ total, items: users, canCreate }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:users.reloaded');
                }

                this.next(s => {
                    const usersPager = s.usersPager.setCount(total);

                    let selectedUser = s.selectedUser;

                    if (selectedUser) {
                        selectedUser = users.find(x => x.id === selectedUser!.id) || selectedUser;
                    }

                    return { ...s,
                        canCreate,
                        isLoaded: true,
                        isLoading: false,
                        selectedUser,
                        users,
                        usersPager
                    };
                });
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request).pipe(
            tap(created => {
                this.next(s => {
                    const users = [created, ...s.users];
                    const usersPager = s.usersPager.incrementCount();

                    return { ...s, users, usersPager };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public update(user: UserDto, request: UpdateUserDto): Observable<UserDto> {
        return this.usersService.putUser(user, request).pipe(
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs));
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

    public search(query: string): Observable<UsersResult> {
        this.next(s => ({ ...s, usersPager: s.usersPager.reset(), usersQuery: query }));

        return this.loadInternal(false);
    }

    public setPager(usersPager: Pager) {
        this.next({ usersPager });

        return this.loadInternal(false);
    }

    private replaceUser(user: UserDto) {
        return this.next(s => {
            const users = s.users.map(u => u.id === user.id ? user : u);

            const selectedUser =
                s.selectedUser?.id !== user.id ?
                s.selectedUser :
                users.find(x => x.id === user.id);

            return { ...s, users, selectedUser };
        });
    }
}
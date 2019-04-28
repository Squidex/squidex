/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, map, tap } from 'rxjs/operators';

import '@app/framework/utils/rxjs-extensions';

import {
    AuthService,
    DialogService,
    ImmutableArray,
    Pager,
    shareSubscribed,
    State
} from '@app/shared';

import {
    CreateUserDto,
    UpdateUserDto,
    UserDto,
    UsersService
} from './../services/users.service';

export interface SnapshotUser {
    // The user.
    user: UserDto;

    // Indicates if the user is the current user.
    isCurrentUser: boolean;
}

interface Snapshot {
    // The current users.
    users: UsersList;

    // The pagination information.
    usersPager: Pager;

    // The query to filter users.
    usersQuery?: string;

    // Indicates if the users are loaded.
    isLoaded?: boolean;

    // The selected user.
    selectedUser?: SnapshotUser | null;
}

export type UsersList = ImmutableArray<SnapshotUser>;
export type UsersResult = { total: number, users: UsersList };

@Injectable()
export class UsersState extends State<Snapshot> {
    public users =
        this.changes.pipe(map(x => x.users),
            distinctUntilChanged());

    public usersPager =
        this.changes.pipe(map(x => x.usersPager),
            distinctUntilChanged());

    public selectedUser =
        this.changes.pipe(map(x => x.selectedUser),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly usersService: UsersService
    ) {
        super({ users: ImmutableArray.empty(), usersPager: new Pager(0) });
    }

    public select(id: string | null): Observable<SnapshotUser | null> {
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

        const found = this.snapshot.users.find(x => x.user.id === id);

        if (found) {
            return of(found);
        }

        return this.usersService.getUser(id).pipe(map(x => this.createUser(x)), catchError(() => of(null)));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false): Observable<any> {
        return this.usersService.getUsers(
                this.snapshot.usersPager.pageSize,
                this.snapshot.usersPager.skip,
                this.snapshot.usersQuery).pipe(
            tap(({ total, items }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Users reloaded.');
                }

                this.next(s => {
                    const usersPager = s.usersPager.setCount(total);
                    const users = ImmutableArray.of(items.map(x => this.createUser(x)));

                    let selectedUser = s.selectedUser;

                    if (selectedUser) {
                        selectedUser = users.find(x => x.user.id === selectedUser!.user.id) || selectedUser;
                    }

                    return { ...s, users, usersPager, selectedUser, isLoaded: true };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request).pipe(
            tap(payload => {
                this.next(s => {
                    const users = s.users.pushFront(this.createUser(payload));
                    const usersPager = s.usersPager.incrementCount();

                    return { ...s, users, usersPager };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public update(user: UserDto, request: UpdateUserDto): Observable<UserDto> {
        return this.usersService.putUser(user.id, request).pipe(
            map(() => update(user, request)),
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public lock(user: UserDto): Observable<UserDto> {
        return this.usersService.lockUser(user.id).pipe(
            map(() => setLocked(user, true)),
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public unlock(user: UserDto): Observable<UserDto> {
        return this.usersService.unlockUser(user.id).pipe(
            map(() => setLocked(user, false)),
            tap(updated => {
                this.replaceUser(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public search(query: string): Observable<UsersResult> {
        this.next(s => ({ ...s, usersPager: new Pager(0), usersQuery: query }));

        return this.loadInternal();
    }

    public goNext(): Observable<UsersResult> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goNext() }));

        return this.loadInternal();
    }

    public goPrev(): Observable<UsersResult> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goPrev() }));

        return this.loadInternal();
    }

    private replaceUser(user: UserDto) {
        return this.next(s => {
            const users = s.users.map(u => u.user.id === user.id ? this.createUser(user) : u);

            const selectedUser =
                s.selectedUser &&
                s.selectedUser.user.id !== user.id ?
                s.selectedUser :
                users.find(x => x.user.id === user.id);

            return { ...s, users, selectedUser };
        });
    }

    private get userId() {
        return this.authState.user!.id;
    }

    private createUser(user: UserDto): SnapshotUser {
        return { user, isCurrentUser: user.id === this.userId };
    }
}


const update = (user: UserDto, request: UpdateUserDto) =>
    user.with(request);

const setLocked = (user: UserDto, isLocked: boolean) =>
    user.with({ isLocked });
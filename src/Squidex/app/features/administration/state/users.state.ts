/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, map, switchMap, tap } from 'rxjs/operators';

import '@app/framework/utils/rxjs-extensions';

import {
    AuthService,
    DialogService,
    ImmutableArray,
    notify,
    Pager,
    State
} from '@app/shared';

import {
    CreateUserDto,
    UpdateUserDto,
    UserDto,
    UsersService
} from './../services/users.service';

interface SnapshotUser {
    // The user.
    user: UserDto;

    // Indicates if the user is the current user.
    isCurrentUser: boolean;
}

interface Snapshot {
    // The current users.
    users: ImmutableArray<SnapshotUser>;

    // The pagination information.
    usersPager: Pager;

    // The query to filter users.
    usersQuery?: string;

    // Indicates if the users are loaded.
    isLoaded?: boolean;

    // The selected user.
    selectedUser?: SnapshotUser | null;
}

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

    public select(id: string | null): Observable<UserDto | null> {
        return this.loadUser(id).pipe(
            tap(selectedUser => {
                this.next(s => ({ ...s, selectedUser }));
            }),
            map(x => x && x.user));
    }

    private loadUser(id: string | null) {
        return !id ?
            of(null) :
            of(this.snapshot.users.find(x => x.user.id === id)).pipe(
                switchMap(user => {
                    if (!user) {
                        return this.usersService.getUser(id).pipe(map(x => this.createUser(x)), catchError(() => of(null)));
                    } else {
                        return of(user);
                    }
                }));
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
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Users reloaded.');
                }

                this.next(s => {
                    const users = ImmutableArray.of(dtos.items.map(x => this.createUser(x)));
                    const usersPager = s.usersPager.setCount(dtos.total);

                    let selectedUser = s.selectedUser;

                    if (selectedUser) {
                        selectedUser = users.find(x => x.user.id === selectedUser!.user.id) || selectedUser;
                    }

                    return { ...s, users, usersPager, selectedUser, isLoaded: true };
                });
            }),
            notify(this.dialogs));
    }

    public create(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request).pipe(
            tap(dto => {
                this.next(s => {
                    const users = s.users.pushFront(this.createUser(dto));
                    const usersPager = s.usersPager.incrementCount();

                    return { ...s, users, usersPager };
                });
            }));
    }

    public update(user: UserDto, request: UpdateUserDto): Observable<any> {
        return this.usersService.putUser(user.id, request).pipe(
            tap(() => {
                this.replaceUser(update(user, request));
            }));
    }

    public lock(user: UserDto): Observable<any> {
        return this.usersService.lockUser(user.id).pipe(
            tap(() => {
                this.replaceUser(setLocked(user, true));
            }),
            notify(this.dialogs));
    }

    public unlock(user: UserDto): Observable<any> {
        return this.usersService.unlockUser(user.id).pipe(
            tap(() => {
                this.replaceUser(setLocked(user, false));
            }),
            notify(this.dialogs));
    }

    public search(query: string): Observable<any> {
        this.next(s => ({ ...s, usersPager: new Pager(0), usersQuery: query }));

        return this.loadInternal();
    }

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goNext() }));

        return this.loadInternal();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goPrev() }));

        return this.loadInternal();
    }

    private replaceUser(user: UserDto) {
        return this.next(s => {
            const users = s.users.map(u => u.user.id === user.id ? this.createUser(user, u) : u);

            const selectedUser = s.selectedUser && s.selectedUser.user.id === user.id ? users.find(x => x.user.id === user.id) : s.selectedUser;

            return { ...s, users, selectedUser };
        });
    }

    private get userId() {
        return this.authState.user!.id;
    }

    private createUser(user: UserDto, current?: SnapshotUser): SnapshotUser {
        if (!user) {
            return null!;
        } else if (current && current.user === user) {
            return current;
        } else {
            return { user, isCurrentUser: user.id === this.userId };
        }
    }
}


const update = (user: UserDto, request: UpdateUserDto) =>
    user.with(request);

const setLocked = (user: UserDto, isLocked: boolean) =>
    user.with({ isLocked });
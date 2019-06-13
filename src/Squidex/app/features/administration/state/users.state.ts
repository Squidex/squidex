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
    DialogService,
    ImmutableArray,
    Pager,
    ResourceLinks,
    shareSubscribed,
    State
} from '@app/shared';

import {
    CreateUserDto,
    UpdateUserDto,
    UserDto,
    UsersService
} from './../services/users.service';

interface Snapshot {
    // The current users.
    users: UsersList;

    // The pagination information.
    usersPager: Pager;

    // The query to filter users.
    usersQuery?: string;

    // The resource links.
    links: ResourceLinks;

    // Indicates if the users are loaded.
    isLoaded?: boolean;

    // The selected user.
    selectedUser?: UserDto | null;
}

export type UsersList = ImmutableArray<UserDto>;
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

    public links =
        this.changes.pipe(map(x => x.links),
            distinctUntilChanged());

    constructor(
        private readonly dialogs: DialogService,
        private readonly usersService: UsersService
    ) {
        super({ users: ImmutableArray.empty(), usersPager: new Pager(0), links: {} });
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

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            const selectedUser = this.snapshot.selectedUser;

            this.resetState({ selectedUser });
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false): Observable<any> {
        return this.usersService.getUsers(
                this.snapshot.usersPager.pageSize,
                this.snapshot.usersPager.skip,
                this.snapshot.usersQuery).pipe(
            tap(({ total, items, _links: links }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Users reloaded.');
                }

                this.next(s => {
                    const usersPager = s.usersPager.setCount(total);
                    const users = ImmutableArray.of(items);

                    let selectedUser = s.selectedUser;

                    if (selectedUser) {
                        selectedUser = users.find(x => x.id === selectedUser!.id) || selectedUser;
                    }

                    return { ...s, users, usersPager, selectedUser, isLoaded: true, links };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request).pipe(
            tap(created => {
                this.next(s => {
                    const users = s.users.pushFront(created);
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
            const users = s.users.map(u => u.id === user.id ? user : u);

            const selectedUser =
                s.selectedUser &&
                s.selectedUser.id !== user.id ?
                s.selectedUser :
                users.find(x => x.id === user.id);

            return { ...s, users, selectedUser };
        });
    }
}
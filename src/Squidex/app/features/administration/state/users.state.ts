/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

import 'framework/utils/rxjs-extensions';

import {
    DialogService,
    ImmutableArray,
    Pager
} from 'shared';

import {
    CreateUserDto,
    UserDto,
    UsersService
} from './../services/users.service';

@Injectable()
export class UsersState {
    public users = new BehaviorSubject<ImmutableArray<UserDto>>(ImmutableArray.empty());
    public usersPager = new BehaviorSubject(new Pager(0));
    public usersQuery = new BehaviorSubject('');

    public selectedUser = new BehaviorSubject<UserDto | null>(null);

    constructor(
        private readonly usersService: UsersService,
        private readonly dialogs: DialogService
    ) {
    }

    public selectUser(id: string | null): Observable<UserDto | null> {
        return this.loadUser(id)
            .do(user => {
                this.selectedUser.next(user);
            });
    }

    private loadUser(id: string | null) {
        return !id ?
            Observable.of(null) :
            Observable.of(this.users.value.find(x => x.id === id))
                .switchMap(user => {
                    if (!user) {
                        return this.usersService.getUser(id).catch(() => Observable.of(null));
                    } else {
                        return Observable.of(user);
                    }
                });
    }

    public loadUsers(): Observable<any> {
        return this.usersService.getUsers(this.usersPager.value.pageSize, this.usersPager.value.skip, this.usersQuery.value)
            .catch(error => this.dialogs.notifyError(error))
            .do(dtos => {
                this.users.nextBy(v => ImmutableArray.of(dtos.items));
                this.usersPager.nextBy(v => v.setCount(dtos.total));
            });
    }

    public createUser(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request)
            .do(dto => {
                this.users.nextBy(v => v.pushFront(dto));
                this.usersPager.nextBy(v => v.incrementCount());
            });
    }

    public updateUser(user: UserDto, request: CreateUserDto): Observable<any> {
        return this.usersService.putUser(user.id, request)
            .do(() => {
                this.dialogs.notifyInfo('User saved successsfull');

                this.replaceUser(user.update(request.email, request.displayName));
            });
    }

    public lockUser(user: UserDto): Observable<any> {
        return this.usersService.lockUser(user.id)
            .catch(error => this.dialogs.notifyError(error))
            .do(() => {
                this.replaceUser(user.lock());
            });
    }

    public unlockUser(user: UserDto): Observable<any> {
        return this.usersService.unlockUser(user.id)
            .catch(error => this.dialogs.notifyError(error))
            .do(() => {
                this.replaceUser(user.unlock());
            });
    }

    public search(filter: string): Observable<any> {
        this.usersPager.nextBy(v => new Pager(0));
        this.usersQuery.nextBy(v => filter);

        return this.loadUsers();
    }

    public goNext(): Observable<any> {
        this.usersPager.nextBy(v => v.goNext());

        return this.loadUsers();
    }

    public goPrev(): Observable<any> {
        this.usersPager.nextBy(v => v.goPrev());

        return this.loadUsers();
    }

    public trackByUser(index: number, user: UserDto): any {
        return user.id;
    }

    private replaceUser(user: UserDto) {
        this.users.nextBy(v => v.replaceBy('id', user));

        this.selectedUser.nextBy(v => v !== null && v.id === user.id ? user : v);
    }
}
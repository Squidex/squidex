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
    ErrorDto,
    ImmutableArray,
    Notification,
    Pager
} from 'shared';

import {
    CreateUserDto,
    UserDto,
    UsersService
} from './../services/users.service';

@Injectable()
export class UsersState {
    public usersItems = new BehaviorSubject<ImmutableArray<UserDto>>(ImmutableArray.empty());
    public usersPager = new BehaviorSubject(new Pager(0));
    public usersQuery = new BehaviorSubject('');

    public selectedUser = new BehaviorSubject<UserDto | null>(null);

    constructor(
        private readonly usersService: UsersService,
        private readonly dialogs: DialogService
    ) {
    }

    public selectUser(id: string | null): Observable<boolean> {
        const observable =
            !id ?
            Observable.of(null) :
            Observable.of(this.usersItems.value.find(x => x.id === id))
                .switchMap(user => {
                    if (!user) {
                        return this.usersService.getUser(id).catch(() => Observable.of(null));
                    } else {
                        return Observable.of(user);
                    }
                });

        return observable
            .do(user => {
                this.selectedUser.next(user);
            })
            .map(u => u !== null);
    }

    public loadUsers(): Observable<any> {
        return this.usersService.getUsers(this.usersPager.value.pageSize, this.usersPager.value.skip, this.usersQuery.value)
            .catch(error => this.notifyError(error))
            .do(dtos => {
                this.usersItems.nextBy(v => ImmutableArray.of(dtos.items));
                this.usersPager.nextBy(v => v.setCount(dtos.total));
            });
    }

    public updateUser(request: CreateUserDto): Observable<any> {
        const id = this.selectedUser.value!.id;

        return this.usersService.putUser(id, request)
            .do(() => {
                this.dialogs.notify(Notification.info('User saved successsfull'));

                this.usersItems.nextBy(v => v.replaceAll(x => x.id === id, u => u.update(request.email, request.displayName)));
            });
    }

    public createUser(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request)
            .catch(error => this.notifyError(error))
            .do(user => {
                this.usersItems.nextBy(v => v.pushFront(user));
                this.usersPager.nextBy(v => v.incrementCount());
            });
    }

    public lockUser(id: string): Observable<any> {
        return this.usersService.lockUser(id)
            .catch(error => this.notifyError(error))
            .do(() => {
                this.usersItems.nextBy(v => v.replaceAll(x => x.id === id, u => u.lock()));
            });
    }

    public unlockUser(id: string): Observable<any> {
        return this.usersService.lockUser(id)
            .catch(error => this.notifyError(error))
            .do(() => {
                this.usersItems.nextBy(v => v.replaceAll(x => x.id === id, u => u.unlock()));
            });
    }

    public search(filter: string) {
        this.usersPager.nextBy(v => new Pager(0));
        this.usersQuery.nextBy(v => filter);

        this.loadUsers();
    }

    public goNext() {
        this.usersPager.nextBy(v => v.goNext());

        this.loadUsers();
    }

    public goPrev() {
        this.usersPager.nextBy(v => v.goPrev());

        this.loadUsers();
    }

    public trackBy(index: number, user: UserDto): any {
        return user.id;
    }

    private notifyError(error: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            this.dialogs.notify(Notification.error(error.displayMessage));
        } else {
            this.dialogs.notify(Notification.error(error));
        }

        return Observable.throw(error);
    }
}
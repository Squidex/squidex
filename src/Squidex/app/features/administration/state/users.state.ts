/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    AuthService,
    DialogService,
    Pager,
    Form,
    State,
    ValidatorsEx
} from '@app/shared';

import {
    CreateUserDto,
    UserDto,
    UsersService
} from './../services/users.service';

export class UserForm extends Form<FormGroup> {
    constructor(
        formBuilder: FormBuilder
    ) {
        super(formBuilder.group({
            email: ['',
                [
                    Validators.email,
                    Validators.required,
                    Validators.maxLength(100)
                ]
            ],
            displayName: ['',
                [
                    Validators.required,
                    Validators.maxLength(100)
                ]
            ],
            password: ['',
                [
                    Validators.nullValidator
                ]
            ],
            passwordConfirm: ['',
                [
                    ValidatorsEx.match('password', 'Passwords must be the same.')
                ]
            ]
        }));
    }

    public load(user?: UserDto) {
        if (user) {
            this.form.controls['password'].setValidators(null);
        } else {
            this.form.controls['password'].setValidators(Validators.required);
        }

        super.load(user);
    }
}

interface Snapshot {
    users: UserDto[];
    usersPager: Pager;
    usersQuery?: string;
    selectedUserId?: string | null;
    isCurrentUser?: boolean;
}

@Injectable()
export class UsersState extends State<Snapshot> {
    constructor(
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly usersService: UsersService
    ) {
        super({ users: [], usersPager: new Pager(10) });
    }

    public selectUser(selectedUserId: string | null): Observable<UserDto | null> {
        return this.loadUser(selectedUserId)
            .do(user => {
                const isCurrentUser = selectedUserId === this.authState.user!.id;

                this.next(s => ({...s, selectedUserId, isCurrentUser }));
            });
    }

    private loadUser(id: string | null) {
        return !id ?
            Observable.of(null) :
            Observable.of(this.snapshot.users.find(x => x.id === id))
                .switchMap(user => {
                    if (!user) {
                        return this.usersService.getUser(id).catch(() => Observable.of(null));
                    } else {

                        return Observable.of(user);
                    }
                });
    }

    public loadUsers(notify = false): Observable<any> {
        return this.usersService.getUsers(this.snapshot.usersPager.pageSize, this.snapshot.usersPager.skip, this.snapshot.usersQuery)
            .catch(error => this.dialogs.notifyError(error))
            .do(dtos => {
                if (notify) {
                    this.dialogs.notifyInfo('Users reloaded.');
                }

                this.next(s => {
                    const users = dtos.items;
                    const usersPager = s.usersPager.setCount(dtos.total);

                    return { ...s, users, usersPager, usersLoading: false };
                });
            });
    }

    public createUser(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request)
            .do(dto => {
                this.next(s => {
                    const users = [dto, ...s.users];
                    const usersPager = s.usersPager.incrementCount();

                    return { ...s, users, usersPager };
                });
            });
    }

    public updateUser(user: UserDto, request: CreateUserDto): Observable<any> {
        return this.usersService.putUser(user.id, request)
            .do(() => {
                this.dialogs.notifyInfo('User saved successsfull');

                this.next(s => {
                    const users = s.users.map(u => u.id === user.id ? u.update(request.email, request.displayName) : u);

                    return { ...s, users };
                });
            });
    }

    public lockUser(user: UserDto): Observable<any> {
        return this.usersService.lockUser(user.id)
            .catch(error => this.dialogs.notifyError(error))
            .do(() => {
                this.next(s => {
                    const users = s.users.map(u => u.id === user.id ? u.lock() : u);

                    return { ...s, users };
                });
            });
    }

    public unlockUser(user: UserDto): Observable<any> {
        return this.usersService.unlockUser(user.id)
            .catch(error => this.dialogs.notifyError(error))
            .do(() => {
                this.next(s => {
                    const users = s.users.map(u => u.id === user.id ? u.unlock() : u);

                    return { ...s, users };
                });
            });
    }

    public search(query: string): Observable<any> {
        this.next({ usersPager: new Pager(0), usersQuery: query });

        return this.loadUsers();
    }

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goNext() }));

        return this.loadUsers();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goPrev() }));

        return this.loadUsers();
    }
}

export const getSelectedUser = (c: Snapshot) => c.users.find(x => x.id === c.selectedUserId);
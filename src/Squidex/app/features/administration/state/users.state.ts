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
    ImmutableArray,
    Pager,
    Form,
    State,
    ValidatorsEx
} from '@app/shared';

import {
    CreateUserDto,
    UserDto,
    UsersService,
    UpdateUserDto
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
    isCurrentUser?: boolean;

    users: ImmutableArray<UserDto>;
    usersPager: Pager;
    usersQuery?: string;

    selectedUser?: UserDto;
}

@Injectable()
export class UsersState extends State<Snapshot> {
    public users =
        this.changes.map(x => x.users)
            .distinctUntilChanged();

    public usersPager =
        this.changes.map(x => x.usersPager)
            .distinctUntilChanged();

    public selectedUser =
        this.changes.map(x => x.selectedUser)
            .distinctUntilChanged();

    public isCurrentUser =
        this.changes.map(x => x.isCurrentUser)
            .distinctUntilChanged();

    constructor(
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly usersService: UsersService
    ) {
        super({ users: ImmutableArray.empty(), usersPager: new Pager(0) });
    }

    public select(id: string | null): Observable<UserDto | null> {
        return this.loadUser(id)
            .do(selectedUser => {
                const isCurrentUser = id === this.authState.user!.id;

                this.next(s => ({ ...s, selectedUser, isCurrentUser }));
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

    public load(notify = false): Observable<any> {
        return this.usersService.getUsers(this.snapshot.usersPager.pageSize, this.snapshot.usersPager.skip, this.snapshot.usersQuery)
            .do(dtos => {
                if (notify) {
                    this.dialogs.notifyInfo('Users reloaded.');
                }

                this.next(s => {
                    const users = ImmutableArray.of(dtos.items);
                    const usersPager = s.usersPager.setCount(dtos.total);

                    let selectedUser = s.selectedUser;

                    if (selectedUser) {
                        const selectedFromResult = dtos.items.find(x => x.id === selectedUser!.id);

                        if (selectedFromResult) {
                            selectedUser = selectedFromResult;
                        }
                    }

                    return { ...s, users, usersPager, selectedUser };
                });
            })
            .notify(this.dialogs);
    }

    public create(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request)
            .do(dto => {
                this.next(s => {
                    const users = s.users.pushFront(dto);
                    const usersPager = s.usersPager.incrementCount();

                    return { ...s, users, usersPager };
                });
            });
    }

    public update(user: UserDto, request: UpdateUserDto): Observable<any> {
        return this.usersService.putUser(user.id, request)
            .do(() => {
                this.dialogs.notifyInfo('User saved successsfull');

                this.replaceUser(update(user, request));
            });
    }

    public lock(user: UserDto): Observable<any> {
        return this.usersService.lockUser(user.id)
            .do(() => {
                this.replaceUser(setLocked(user, true));
            })
            .notify(this.dialogs);
    }

    public unlock(user: UserDto): Observable<any> {
        return this.usersService.unlockUser(user.id)
            .do(() => {
                this.replaceUser(setLocked(user, false));
            })
            .notify(this.dialogs);
    }

    public search(query: string): Observable<any> {
        this.next(s => ({ ...s, usersPager: new Pager(0), usersQuery: query }));

        return this.load();
    }

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goNext() }));

        return this.load();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, usersPager: s.usersPager.goPrev() }));

        return this.load();
    }

    private replaceUser(user: UserDto) {
        return this.next(s => {
            const users = s.users.replaceBy('id', user);
            const selectedUser = s.selectedUser && s.selectedUser.id === user.id ? user : s.selectedUser;

            return { ...s, users, selectedUser };
        });
    }
}


const update = (user: UserDto, request: UpdateUserDto) =>
    new UserDto(user.id, request.email, request.displayName, user.isLocked);

const setLocked = (user: UserDto, locked: boolean) =>
    new UserDto(user.id, user.email, user.displayName, locked);
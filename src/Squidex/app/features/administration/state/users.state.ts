/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    AuthService,
    DialogService,
    Form,
    ImmutableArray,
    Pager,
    State,
    ValidatorsEx
} from '@app/shared';

import {
    CreateUserDto,
    UpdateUserDto,
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

interface SnapshotUser {
    user: UserDto;

    isCurrentUser: boolean;
}

interface Snapshot {
    users: ImmutableArray<SnapshotUser>;
    usersPager: Pager;
    usersQuery?: string;

    isLoaded?: boolean;

    selectedUser?: SnapshotUser;
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

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
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
                this.next(s => ({ ...s, selectedUser }));
            })
            .map(x => x && x.user);
    }

    private loadUser(id: string | null) {
        return !id ?
            Observable.of(null) :
            Observable.of(this.snapshot.users.find(x => x.user.id === id))
                .switchMap(user => {
                    if (!user) {
                        return this.usersService.getUser(id).map(x => this.createUser(x)).catch(() => Observable.of(null));
                    } else {
                        return Observable.of(user);
                    }
                });
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
                this.snapshot.usersQuery)
            .do(dtos => {
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
            })
            .notify(this.dialogs);
    }

    public create(request: CreateUserDto): Observable<UserDto> {
        return this.usersService.postUser(request)
            .do(dto => {
                this.next(s => {
                    const users = s.users.pushFront(this.createUser(dto));
                    const usersPager = s.usersPager.incrementCount();

                    return { ...s, users, usersPager };
                });
            });
    }

    public update(user: UserDto, request: UpdateUserDto): Observable<any> {
        return this.usersService.putUser(user.id, request)
            .do(() => {
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
    new UserDto(user.id, request.email, request.displayName, user.isLocked);

const setLocked = (user: UserDto, isLocked: boolean) =>
    new UserDto(user.id, user.email, user.displayName, isLocked);
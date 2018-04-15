/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { AuthService, DialogService } from '@app/shared';

import { UsersState } from './users.state';

import {
    CreateUserDto,
    UserDto,
    UpdateUserDto,
    UsersDto,
    UsersService
} from './../services/users.service';

describe('UsersState', () => {
    const oldUsers = [
        new UserDto('id1', 'mail1@mail.de', 'name1', false),
        new UserDto('id2', 'mail2@mail.de', 'name2', true)
    ];

    const newUser = new UserDto('id3', 'mail3@mail.de', 'name3', false);

    let authService: IMock<AuthService>;
    let dialogs: IMock<DialogService>;
    let usersService: IMock<UsersService>;
    let usersState: UsersState;

    beforeEach(() => {
        authService = Mock.ofType<AuthService>();

        authService.setup(x => x.user)
            .returns(() => <any>{ id: 'id2' });

        dialogs = Mock.ofType<DialogService>();

        usersService = Mock.ofType<UsersService>();

        usersService.setup(x => x.getUsers(10, 0, undefined))
            .returns(() => Observable.of(new UsersDto(200, oldUsers)));

        usersState = new UsersState(authService.object, dialogs.object, usersService.object);
        usersState.load().subscribe();
    });

    it('should load users', () => {
        expect(usersState.snapshot.users.values).toEqual(oldUsers.map(x => u(x)));
        expect(usersState.snapshot.usersPager.numberOfItems).toEqual(200);

        usersService.verifyAll();
    });

    it('should raise notification on load when notify is true', () => {
        usersState.load(true).subscribe();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should replace selected user when reloading', () => {
        usersState.select('id1').subscribe();

        const newUsers = [
            new UserDto('id1', 'mail1@mail.de_new', 'name1_new', false),
            new UserDto('id2', 'mail2@mail.de_new', 'name2_new', true)
        ];

        usersService.setup(x => x.getUsers(10, 0, undefined))
            .returns(() => Observable.of(new UsersDto(200, newUsers)));

        usersState.load().subscribe();

        expect(usersState.snapshot.selectedUser).toEqual(u(newUsers[0]));
    });

    it('should mark as current user when selected user equals to profile', () => {
        let selectedUser: UserDto;

        usersState.select('id2').subscribe(x => {
            selectedUser = x!;
        });

        expect(selectedUser!).toEqual(oldUsers[1]);
        expect(usersState.snapshot.selectedUser).toEqual(u(oldUsers[1]));
    });

    it('should not load user when already loaded', () => {
        let selectedUser: UserDto;

        usersState.select('id1').subscribe(x => {
            selectedUser = x!;
        });

        expect(selectedUser!).toEqual(oldUsers[0]);
        expect(usersState.snapshot.selectedUser).toEqual(u(oldUsers[0]));

        usersService.verify(x => x.getUser(It.isAnyString()), Times.never());
    });

    it('should load user when not loaded', () => {
        usersService.setup(x => x.getUser('id3'))
            .returns(() => Observable.of(newUser));

        let selectedUser: UserDto;

        usersState.select('id3').subscribe(x => {
            selectedUser = x!;
        });

        expect(selectedUser!).toEqual(newUser);
        expect(usersState.snapshot.selectedUser).toEqual(u(newUser));

        usersService.verify(x => x.getUser('id3'), Times.once());
    });

    it('should return null when unselecting user', () => {
        let selectedUser: UserDto;

        usersState.select(null).subscribe(x => {
            selectedUser = x!;
        });

        expect(selectedUser!).toBeNull();
        expect(usersState.snapshot.selectedUser).toBeNull();

        usersService.verify(x => x.getUser(It.isAnyString()), Times.never());
    });

    it('should return null when user to select is not found', () => {
        usersService.setup(x => x.getUser('unknown'))
            .returns(() => Observable.throw({}));

        let selectedUser: UserDto;

        usersState.select('unknown').subscribe(x => {
            selectedUser = x!;
        }).unsubscribe();

        expect(selectedUser!).toBeNull();
        expect(usersState.snapshot.selectedUser).toBeNull();
    });

    it('should mark user as locked', () => {
        usersService.setup(x => x.lockUser('id1'))
            .returns(() => Observable.of({}));

        usersState.select('id1').subscribe();
        usersState.lock(oldUsers[0]).subscribe();

        const user_1 = usersState.snapshot.users.at(0);

        expect(user_1.user.isLocked).toBeTruthy();
        expect(user_1).toBe(usersState.snapshot.selectedUser);
    });

    it('should unmark user as locked', () => {
        usersService.setup(x => x.unlockUser('id2'))
            .returns(() => Observable.of({}));

        usersState.select('id2').subscribe();
        usersState.unlock(oldUsers[1]).subscribe();

        const user_1 = usersState.snapshot.users.at(1);

        expect(user_1.user.isLocked).toBeFalsy();
        expect(user_1).toBe(usersState.snapshot.selectedUser);
    });

    it('should update user on update', () => {
        const request = new UpdateUserDto('new@mail.com', 'New');

        usersService.setup(x => x.putUser('id1', request))
            .returns(() => Observable.of({}));

        usersState.select('id1').subscribe();
        usersState.update(oldUsers[0], request).subscribe();

        const user_1 = usersState.snapshot.users.at(0);

        expect(user_1.user.email).toEqual('new@mail.com');
        expect(user_1.user.displayName).toEqual('New');
        expect(user_1).toBe(usersState.snapshot.selectedUser);
    });

    it('should add user to snapshot when created', () => {
        const request = new CreateUserDto(newUser.email, newUser.displayName, 'password');

        usersService.setup(x => x.postUser(request))
            .returns(() => Observable.of(newUser));

        usersState.create(request).subscribe();

        expect(usersState.snapshot.users.values).toEqual([u(newUser), ...oldUsers.map(x => u(x))]);
        expect(usersState.snapshot.usersPager.numberOfItems).toBe(201);
    });

    it('should load next page and prev page when paging', () => {
        usersService.setup(x => x.getUsers(10, 10, undefined))
            .returns(() => Observable.of(new UsersDto(200, [])));

        usersState.goNext().subscribe();
        usersState.goPrev().subscribe();

        usersService.verify(x => x.getUsers(10, 10, undefined), Times.once());
        usersService.verify(x => x.getUsers(10, 0,  undefined), Times.exactly(2));
    });

    it('should load with query when searching', () => {
        usersService.setup(x => x.getUsers(10, 0, 'my-query'))
            .returns(() => Observable.of(new UsersDto(0, [])));

        usersState.search('my-query').subscribe();

        expect(usersState.snapshot.usersQuery).toEqual('my-query');

        usersService.verify(x => x.getUsers(10, 0, 'my-query'), Times.once());
    });

    function u(user: UserDto) {
        return { user, isCurrentUser: user.id === 'id2' };
    }
});
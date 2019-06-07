/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { AuthService, DialogService } from '@app/shared';

import {
    UserDto,
    UsersDto,
    UsersService
} from '@app/features/administration/internal';

import { UsersState } from './users.state';

describe('UsersState', () => {
    const oldUsers = [
        new UserDto('id1', 'mail1@mail.de', 'name1', ['Permission1'], false),
        new UserDto('id2', 'mail2@mail.de', 'name2', ['Permission2'], true)
    ];

    const newUser = new UserDto('id3', 'mail3@mail.de', 'name3', ['Permission3'], false);

    let dialogs: IMock<DialogService>;
    let usersService: IMock<UsersService>;
    let usersState: UsersState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        usersService = Mock.ofType<UsersService>();
        usersState = new UsersState(dialogs.object, usersService.object);
    });

    afterEach(() => {
        usersService.verifyAll();
    });

    describe('Loading', () => {
        it('should load users', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, oldUsers))).verifiable();

            usersState.load().subscribe();

            expect(usersState.snapshot.users.values).toEqual(oldUsers);
            expect(usersState.snapshot.usersPager.numberOfItems).toEqual(200);
            expect(usersState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, oldUsers))).verifiable();

            usersState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should replace selected user when reloading', () => {
            const newUsers = [
                new UserDto('id1', 'mail1@mail.de_new', 'name1_new', ['Permission1_New'], false),
                new UserDto('id2', 'mail2@mail.de_new', 'name2_new', ['Permission2_New'], true)
            ];

            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, oldUsers))).verifiable(Times.exactly(2));

            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, newUsers)));

            usersState.load().subscribe();
            usersState.select('id1').subscribe();
            usersState.load().subscribe();

            expect(usersState.snapshot.selectedUser).toEqual(newUsers[0]);
        });

        it('should load next page and prev page when paging', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, oldUsers))).verifiable(Times.exactly(2));

            usersService.setup(x => x.getUsers(10, 10, undefined))
                .returns(() => of(new UsersDto(200, []))).verifiable();

            usersState.load().subscribe();
            usersState.goNext().subscribe();
            usersState.goPrev().subscribe();

            expect().nothing();
        });

        it('should load with query when searching', () => {
            usersService.setup(x => x.getUsers(10, 0, 'my-query'))
                .returns(() => of(new UsersDto(0, []))).verifiable();

            usersState.search('my-query').subscribe();

            expect(usersState.snapshot.usersQuery).toEqual('my-query');
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, oldUsers))).verifiable();

            usersState.load().subscribe();
        });

        it('should return user on select and not load when already loaded', () => {
            let selectedUser: UserDto;

            usersState.select('id1').subscribe(x => {
                selectedUser = x!;
            });

            expect(selectedUser!).toEqual(oldUsers[0]);
            expect(usersState.snapshot.selectedUser).toEqual(oldUsers[0]);
        });

        it('should return user on select and load when not loaded', () => {
            usersService.setup(x => x.getUser('id3'))
                .returns(() => of(newUser));

            let selectedUser: UserDto;

            usersState.select('id3').subscribe(x => {
                selectedUser = x!;
            });

            expect(selectedUser!).toEqual(newUser);
            expect(usersState.snapshot.selectedUser).toEqual(newUser);
        });

        it('should return null on select when unselecting user', () => {
            let selectedUser: UserDto;

            usersState.select(null).subscribe(x => {
                selectedUser = x!;
            });

            expect(selectedUser!).toBeNull();
            expect(usersState.snapshot.selectedUser).toBeNull();
        });

        it('should return null on select when user is not found', () => {
            usersService.setup(x => x.getUser('unknown'))
                .returns(() => throwError({})).verifiable();

            let selectedUser: UserDto;

            usersState.select('unknown').subscribe(x => {
                selectedUser = x!;
            }).unsubscribe();

            expect(selectedUser!).toBeNull();
            expect(usersState.snapshot.selectedUser).toBeNull();
        });

        it('should mark as locked when locked', () => {
            usersService.setup(x => x.lockUser('id1'))
                .returns(() => of({})).verifiable();

            usersState.select('id1').subscribe();
            usersState.lock(oldUsers[0]).subscribe();

            const user_1 = usersState.snapshot.users.at(0);

            expect(user_1.isLocked).toBeTruthy();
            expect(user_1).toBe(usersState.snapshot.selectedUser!);
        });

        it('should unmark as locked when unlocked', () => {
            usersService.setup(x => x.unlockUser('id2'))
                .returns(() => of({})).verifiable();

            usersState.select('id2').subscribe();
            usersState.unlock(oldUsers[1]).subscribe();

            const user_1 = usersState.snapshot.users.at(1);

            expect(user_1.isLocked).toBeFalsy();
            expect(user_1).toBe(usersState.snapshot.selectedUser!);
        });

        it('should update user properties when updated', () => {
            const request = { email: 'new@mail.com', displayName: 'New', permissions: ['Permission1'] };

            usersService.setup(x => x.putUser('id1', request))
                .returns(() => of({})).verifiable();

            usersState.select('id1').subscribe();
            usersState.update(oldUsers[0], request).subscribe();

            const user_1 = usersState.snapshot.users.at(0);

            expect(user_1.email).toEqual(request.email);
            expect(user_1.displayName).toEqual(request.displayName);
            expect(user_1.permissions).toEqual(request.permissions);
            expect(user_1).toBe(usersState.snapshot.selectedUser!);
        });

        it('should add user to snapshot when created', () => {
            const request = { ...newUser, password: 'password' };

            usersService.setup(x => x.postUser(request))
                .returns(() => of(newUser)).verifiable();

            usersState.create(request).subscribe();

            expect(usersState.snapshot.users.values).toEqual([newUser, ...oldUsers]);
            expect(usersState.snapshot.usersPager.numberOfItems).toBe(201);
        });
    });
});
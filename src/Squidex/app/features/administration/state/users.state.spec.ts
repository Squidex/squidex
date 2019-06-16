/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { DialogService } from '@app/shared';

import {
    UserDto,
    UsersDto,
    UsersService
} from '@app/features/administration/internal';

import { UsersState } from './users.state';

import { createUser } from './../services/users.service.spec';

describe('UsersState', () => {
    const user1 = createUser(1);
    const user2 = createUser(2);

    const oldUsers = new UsersDto(200, [user1, user2]);

    const newUser = createUser(3);

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
                .returns(() => of(oldUsers)).verifiable();

            usersState.load().subscribe();

            expect(usersState.snapshot.users.values).toEqual([user1, user2]);
            expect(usersState.snapshot.usersPager.numberOfItems).toEqual(200);
            expect(usersState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(oldUsers)).verifiable();

            usersState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should replace selected user when reloading', () => {
            const newUsers = [
                createUser(1, '_new'),
                createUser(2, '_new')
            ];

            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(oldUsers)).verifiable(Times.exactly(2));

            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(200, newUsers)));

            usersState.load().subscribe();
            usersState.select(user1.id).subscribe();
            usersState.load().subscribe();

            expect(usersState.snapshot.selectedUser).toEqual(newUsers[0]);
        });

        it('should load next page and prev page when paging', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(oldUsers)).verifiable(Times.exactly(2));

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
                .returns(() => of(oldUsers)).verifiable();

            usersState.load().subscribe();
        });

        it('should return user on select and not load when already loaded', () => {
            let selectedUser: UserDto;

            usersState.select(user1.id).subscribe(x => {
                selectedUser = x!;
            });

            expect(selectedUser!).toEqual(user1);
            expect(usersState.snapshot.selectedUser).toEqual(user1);
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

        it('should update user and selected user when locked', () => {
            const updated = createUser(2, '_new');

            usersService.setup(x => x.lockUser(user2))
                .returns(() => of(updated)).verifiable();

            usersState.select(user2.id).subscribe();
            usersState.lock(user2).subscribe();

            const user2New = usersState.snapshot.users.at(1);

            expect(user2New).toBe(usersState.snapshot.selectedUser!);
        });

        it('should update user and selected user when unlocked', () => {
            const updated = createUser(2, '_new');

            usersService.setup(x => x.unlockUser(user2))
                .returns(() => of(updated)).verifiable();

            usersState.select(user2.id).subscribe();
            usersState.unlock(user2).subscribe();

            const user2New = usersState.snapshot.users.at(1);

            expect(user2New).toEqual(updated);
            expect(user2New).toBe(usersState.snapshot.selectedUser!);
        });

        it('should update user and selected user when updated', () => {
            const request = { email: 'new@mail.com', displayName: 'New', permissions: ['Permission1'] };

            const updated = createUser(2, '_new');

            usersService.setup(x => x.putUser(user2, request))
                .returns(() => of(updated)).verifiable();

            usersState.select(user2.id).subscribe();
            usersState.update(user2, request).subscribe();

            const user2New = usersState.snapshot.users.at(1);

            expect(user2New).toEqual(updated);
            expect(user2New).toBe(usersState.snapshot.selectedUser!);
        });

        it('should add user to snapshot when created', () => {
            const request = { ...newUser, password: 'password' };

            usersService.setup(x => x.postUser(request))
                .returns(() => of(newUser)).verifiable();

            usersState.create(request).subscribe();

            expect(usersState.snapshot.users.values).toEqual([newUser, user1, user2]);
            expect(usersState.snapshot.usersPager.numberOfItems).toBe(201);
        });
    });
});
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UserDto, UsersDto, UsersService } from '@app/features/administration/internal';
import { DialogService } from '@app/shared';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { createUser } from './../services/users.service.spec';
import { UsersState } from './users.state';

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

            expect(usersState.snapshot.users).toEqual([user1, user2]);
            expect(usersState.snapshot.isLoaded).toBeTruthy();
            expect(usersState.snapshot.isLoading).toBeFalsy();
            expect(usersState.snapshot.total).toEqual(200);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => throwError(() => 'Service Error'));

            usersState.load().pipe(onErrorResumeNext()).subscribe();

            expect(usersState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(oldUsers)).verifiable();

            usersState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load with new pagination if paging', () => {
            usersService.setup(x => x.getUsers(10, 10, undefined))
                .returns(() => of(new UsersDto(200, []))).verifiable();

            usersState.page({ page: 1, pageSize: 10 }).subscribe();

            expect().nothing();
        });

        it('should load with query if searching', () => {
            usersService.setup(x => x.getUsers(10, 0, 'my-query'))
                .returns(() => of(new UsersDto(0, []))).verifiable();

            usersState.search('my-query').subscribe();

            expect(usersState.snapshot.query).toEqual('my-query');
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(oldUsers)).verifiable();

            usersState.load().subscribe();
        });

        it('should return user on select and not load if already loaded', () => {
            let selectedUser: UserDto;

            usersState.select(user1.id).subscribe(x => {
                selectedUser = x!;
            });

            expect(selectedUser!).toEqual(user1);
            expect(usersState.snapshot.selectedUser).toEqual(user1);
        });

        it('should return user on select and load if not loaded', () => {
            usersService.setup(x => x.getUser('id3'))
                .returns(() => of(newUser));

            let userSelected: UserDto;

            usersState.select('id3').subscribe(x => {
                userSelected = x!;
            });

            expect(userSelected!).toEqual(newUser);
            expect(usersState.snapshot.selectedUser).toEqual(newUser);
        });

        it('should return null on select if unselecting user', () => {
            let userSelected: UserDto;

            usersState.select(null).subscribe(x => {
                userSelected = x!;
            });

            expect(userSelected!).toBeNull();
            expect(usersState.snapshot.selectedUser).toBeNull();
        });

        it('should return null on select if user is not found', () => {
            usersService.setup(x => x.getUser('unknown'))
                .returns(() => throwError(() => 'Service Error')).verifiable();

            let userSelected: UserDto;

            usersState.select('unknown').pipe(onErrorResumeNext()).subscribe(x => {
                userSelected = x!;
            }).unsubscribe();

            expect(userSelected!).toBeNull();
            expect(usersState.snapshot.selectedUser).toBeNull();
        });

        it('should add user to snapshot if created', () => {
            const request = { ...newUser, password: 'password' };

            usersService.setup(x => x.postUser(request))
                .returns(() => of(newUser)).verifiable();

            usersState.create(request).subscribe();

            expect(usersState.snapshot.users).toEqual([newUser, user1, user2]);
            expect(usersState.snapshot.total).toBe(201);
        });

        it('should update user if updated', () => {
            const request = {};

            const updated = createUser(2, '_new');

            usersService.setup(x => x.putUser(user2, request))
                .returns(() => of(updated)).verifiable();

            usersState.update(user2, request).subscribe();

            expect(usersState.snapshot.users).toEqual([user1, updated]);
        });

        it('should update user if locked', () => {
            const updated = createUser(2, '_new');

            usersService.setup(x => x.lockUser(user2))
                .returns(() => of(updated)).verifiable();

            usersState.lock(user2).subscribe();

            expect(usersState.snapshot.users).toEqual([user1, updated]);
        });

        it('should update user if locked', () => {
            const updated = createUser(2, '_new');

            usersService.setup(x => x.unlockUser(user2))
                .returns(() => of(updated)).verifiable();

            usersState.unlock(user2).subscribe();

            expect(usersState.snapshot.users).toEqual([user1, updated]);
        });

        it('should remove user from snapshot if delete', () => {
            usersService.setup(x => x.deleteUser(user1))
                .returns(() => of(newUser)).verifiable();

            usersState.delete(user1).subscribe();

            expect(usersState.snapshot.users).toEqual([user2]);
            expect(usersState.snapshot.total).toBe(199);
        });

        it('should truncate users if page size reached', () => {
            const request = { ...newUser, password: 'password' };

            usersService.setup(x => x.getUsers(2, 0, undefined))
                .returns(() => of(new UsersDto(200, [user1, user2]))).verifiable();

            usersService.setup(x => x.postUser(request))
                .returns(() => of(newUser)).verifiable();

            usersState.page({ page: 0, pageSize: 2 }).subscribe();
            usersState.create(request).subscribe();

            expect(usersState.snapshot.users).toEqual([newUser, user1]);
            expect(usersState.snapshot.total).toBe(201);
        });
    });

    describe('Selection', () => {
        beforeEach(() => {
            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(oldUsers)).verifiable(Times.atLeastOnce());

            usersState.load().subscribe();
            usersState.select(user2.id).subscribe();
        });

        it('should update selected user if reloaded', () => {
            const newUsers = [
                createUser(1, '_new'),
                createUser(2, '_new'),
            ];

            usersService.setup(x => x.getUsers(10, 0, undefined))
                .returns(() => of(new UsersDto(2, newUsers)));

            usersState.load().subscribe();

            expect(usersState.snapshot.selectedUser).toEqual(newUsers[1]);
        });

        it('should update selected user if updated', () => {
            const request = {};

            const updated = createUser(2, '_new');

            usersService.setup(x => x.putUser(user2, request))
                .returns(() => of(updated)).verifiable();

            usersState.update(user2, request).subscribe();

            expect(usersState.snapshot.selectedUser).toEqual(updated);
        });

        it('should remove selected user from snapshot if deleted', () => {
            usersService.setup(x => x.deleteUser(user2))
                .returns(() => of(true)).verifiable();

            usersState.delete(user2).subscribe();

            expect(usersState.snapshot.selectedUser).toBeNull();
        });
    });
});

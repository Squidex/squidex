/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { AuthService, DialogService } from '@app/shared';

import { SchemasState } from './schemas.state';

import {
    AppsState,
    DateTime,
    SchemaDetailsDto,
    SchemaDto,
    SchemasService,
    Version
 } from '@app/shared';

describe('SchemasState', () => {
    const app = 'my-app';

    const yesterday = DateTime.today().addDays(-1);

    const oldSchemas = [
        new SchemaDto('id1', 'name1', {}, true, 'me', 'me', yesterday, yesterday, new Version('1')),
        new SchemaDto('id2', 'name2', {}, true, 'me', 'me', yesterday, yesterday, new Version('2'))
    ];

    const schema = new SchemaDetailsDto('id2', 'name2', {}, true, 'me', 'me', yesterday, yesterday, new Version('2'), []);

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let authService: IMock<AuthService>;
    let schemasService: IMock<SchemasService>;
    let schemasState: SchemasState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        authService = Mock.ofType<AuthService>();

        authService.setup(x => x.user)
            .returns(() => <any>{ id: 'id2' });

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        schemasService = Mock.ofType<SchemasService>();

        schemasService.setup(x => x.getSchemas(app))
            .returns(() => Observable.of(oldSchemas));

        schemasService.setup(x => x.getSchema(app, 'name2'))
            .returns(() => Observable.of(schema));

        schemasState = new SchemasState(appsState.object, authService.object, dialogs.object, schemasService.object);
        schemasState.loadSchemas().subscribe();
    });

    it('should load schemas', () => {
        expect(schemasState.snapshot.schemas.values).toEqual(oldSchemas);

        schemasService.verifyAll();
    });

    it('should not load schema when already loaded', () => {
        schemasState.selectSchema('name2').subscribe();
        schemasState.selectSchema('name2').subscribe();

        schemasService.verify(x => x.getSchema(app, 'name2'), Times.once());
    });

    it('should load selected schema when not loaded', () => {
        let selectedSchema: SchemaDetailsDto;

        schemasState.selectSchema('name2').subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBe(schema);
        expect(schemasState.snapshot.selectedSchema).toBe(schema);
        expect(schemasState.snapshot.selectedSchema).toBe(schemasState.snapshot.schemas.at(1));
    });

    it('should return null when unselecting schema', () => {
        let selectedSchema: SchemaDetailsDto;

        schemasState.selectSchema(null).subscribe(x => {
            selectedSchema = x!;
        });

        expect(selectedSchema!).toBeNull();
        expect(schemasState.snapshot.selectedSchema).toBeNull();

        schemasService.verify(x => x.getSchema(app, It.isAnyString()), Times.never());
    });

    /*
    it('should mark as current user when selected user equals to profile', () => {
        usersState.selectUser('id2').subscribe();

        expect(usersState.snapshot.isCurrentUser).toBeTruthy();
    });

    it('should load user when not loaded', () => {
        usersService.setup(x => x.getUser('id3'))
            .returns(() => Observable.of(newUser));

        let selectedUser: UserDto;

        usersState.selectUser('id3').subscribe(x => {
            selectedUser = x!;
        });

        expect(selectedUser!).toEqual(newUser);
        expect(usersState.snapshot.selectedUser).toBe(newUser);
    });

    it('should return null when user to select is not found', () => {
        usersService.setup(x => x.getUser('unknown'))
            .returns(() => Observable.throw({}));

        let selectedUser: UserDto;

        usersState.selectUser('unknown').subscribe(x => {
            selectedUser = x!;
        }).unsubscribe();

        expect(selectedUser!).toBeNull();
        expect(usersState.snapshot.selectedUser).toBeNull();
    });

    it('should mark user as locked', () => {
        usersService.setup(x => x.lockUser('id1'))
            .returns(() => Observable.of({}));

        usersState.selectUser('id1').subscribe();
        usersState.lockUser(oldUsers[0]).subscribe();

        expect(usersState.snapshot.users.at(0).isLocked).toBeTruthy();
        expect(usersState.snapshot.selectedUser).toBe(usersState.snapshot.users.at(0));
    });

    it('should raise notification when locking failed', () => {
        usersService.setup(x => x.lockUser('id1'))
            .returns(() => Observable.throw({}));

        usersState.lockUser(oldUsers[0]).onErrorResumeNext().subscribe();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
    });

    it('should unmark user as locked', () => {
        usersService.setup(x => x.unlockUser('id2'))
            .returns(() => Observable.of({}));

        usersState.selectUser('id2').subscribe();
        usersState.unlockUser(oldUsers[1]).subscribe();

        expect(usersState.snapshot.users.at(1).isLocked).toBeFalsy();
        expect(usersState.snapshot.selectedUser).toBe(usersState.snapshot.users.at(1));
    });

    it('should raise notification when unlocking failed', () => {
        usersService.setup(x => x.unlockUser('id2'))
            .returns(() => Observable.throw({}));

        usersState.unlockUser(oldUsers[1]).onErrorResumeNext().subscribe();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
    });

    it('should update user on update', () => {
        const request = new UpdateUserDto('new@mail.com', 'New');

        usersService.setup(x => x.putUser('id1', request))
            .returns(() => Observable.of({}));

        usersState.selectUser('id1').subscribe();
        usersState.updateUser(oldUsers[0], request).subscribe();

        expect(usersState.snapshot.users.at(0).email).toEqual('new@mail.com');
        expect(usersState.snapshot.users.at(0).displayName).toEqual('New');
        expect(usersState.snapshot.selectedUser).toBe(usersState.snapshot.users.at(0));
    });

    it('should not raise notification when updating failed', () => {
        const request = new UpdateUserDto('new@mail.com', 'New');

        usersService.setup(x => x.putUser('id1', request))
            .returns(() => Observable.throw({}));

        usersState.updateUser(oldUsers[0], request).onErrorResumeNext().subscribe();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
    });

    it('should add user to state when created', () => {
        const request = new CreateUserDto(newUser.email, newUser.displayName, 'password');

        usersService.setup(x => x.postUser(request))
            .returns(() => Observable.of(newUser));

        usersState.createUser(request).subscribe();

        expect(usersState.snapshot.users.at(0)).toBe(newUser);
        expect(usersState.snapshot.usersPager.numberOfItems).toBe(201);
    });

    it('should not raise notification when creating failed', () => {
        const request = new CreateUserDto(newUser.email, newUser.displayName, 'password');

        usersService.setup(x => x.postUser(request))
            .returns(() => Observable.throw({}));

        usersState.createUser(request).onErrorResumeNext().subscribe();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
    });

    it('should load next page and prev page when paging', () => {
        usersService.setup(x => x.getUsers(10, 10, undefined))
            .returns(() => Observable.of(new UsersDto(200, [])));

        usersState.goNext().subscribe();
        usersState.goPrev().subscribe();

        usersService.verify(x => x.getUsers(10, 10, undefined), Times.once());
        usersService.verify(x => x.getUsers(10,  0, undefined), Times.exactly(2));
    });

    it('should load with query when searching', () => {
        usersService.setup(x => x.getUsers(10, 0, 'my-query'))
            .returns(() => Observable.of(new UsersDto(0, [])));

        usersState.search('my-query').subscribe();

        expect(usersState.snapshot.usersQuery).toEqual('my-query');

        usersService.verify(x => x.getUsers(10, 0, 'my-query'), Times.once());
    });*/
});
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { signal, Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { IMock, Mock } from 'typemoq';
import { ApiUrlConfig, UserDto, UsersProviderService } from '@app/shared/internal';
import { ScriptNamePipe, UserNamePipe, UserNameRefPipe, UserPicturePipe, UserPictureRefPipe } from './pipes';

describe('ScriptNamePipe', () => {
    const pipe = new ScriptNamePipe();

    it('should return titlecase for schema name', () => {
        const actual = pipe.transform('create');

        expect(actual).toEqual('Create');
    });

    it('should return custom name for queryPre', () => {
        const actual = pipe.transform('queryPre');

        expect(actual).toEqual('Prepare Query');
    });
});

describe('UserPipes', () => {
    let users: IMock<UsersProviderService>;

    beforeEach(() => {
        users = Mock.ofType<UsersProviderService>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: UsersProviderService,
                    useValue: users.object,
                },
                {
                    provide: ApiUrlConfig,
                    useValue: new ApiUrlConfig('http://service/p/'),
                },
            ],
        });
    });

    it('should return loading text if user not loaded yet', () => {
        users.setup(x => x.user('1', 'Me'))
            .returns(() => signal(undefined));

        const actual = createPipe(UserNamePipe).transform('1');

        expect(actual).toEqual('Loading...');
    });

    it('should return display name of user', () => {
        users.setup(x => x.user('1', 'Me'))
            .returns(() => signal(new UserDto({ id: '1', displayName: 'User1' } as any)));

        const actual = createPipe(UserNamePipe).transform('1');

        expect(actual).toEqual('User1');
    });

    it('should return display name of subject', () => {
        users.setup(x => x.user('1', null))
            .returns(() => signal(new UserDto({ id: '1', displayName: 'User1' } as any)));

        const actual = createPipe(UserNameRefPipe).transform('subject:1', null);

        expect(actual).toEqual('User1');
    });

    it('should return client name for client ref', () => {
        const actual = createPipe(UserNameRefPipe).transform('client:my');

        expect(actual).toEqual('my client');
    });

    it('should return empty picture if user not loaded yet', () => {
        users.setup(x => x.user('1'))
            .returns(() => signal(undefined));

        const actual = createPipe(UserPicturePipe).transform('1');

        expect(actual).toEqual('');
    });

    it('should return picture url of user', () => {
        users.setup(x => x.user('1'))
            .returns(() => signal(new UserDto({ id: '1', displayName: 'User1' } as any)));

        const actual = createPipe(UserPicturePipe).transform('1');

        expect(actual).toEqual('http://service/p/api/users/1/picture');
    });

    it('should return default picture for client ref', () => {
        const actual = createPipe(UserPictureRefPipe).transform('client:my');

        expect(actual).toEqual('./images/squid.svg');
    });

    function createPipe<T>(type: Type<T>) {
        return TestBed.runInInjectionContext(() => new type());
    }
});

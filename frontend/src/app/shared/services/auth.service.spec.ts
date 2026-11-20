/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { User, UserManager } from 'oidc-client-ts';
import { ApiUrlConfig } from '@app/framework';
import { AuthService } from './auth.service';

describe('AuthService', () => {
    const user = new User({
        access_token: 'token1',
        profile: { sub: '123' } as any,
        token_type: 'Bearer',
    });

    beforeEach(() => {
        vi.spyOn(UserManager.prototype, 'getUser').mockResolvedValue(null);

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: ApiUrlConfig,
                    useValue: new ApiUrlConfig('http://service/p/'),
                },
            ],
        });
    });

    afterEach(() => {
        vi.restoreAllMocks();
        vi.useRealTimers();
    });

    it('should return profile if silent login succeeds', async () => {
        const signinSilent = vi.spyOn(UserManager.prototype, 'signinSilent').mockResolvedValue(user);

        const authService = TestBed.inject(AuthService);

        const profile = await authService.loginSilent();

        expect(profile.authorization).toEqual('Bearer token1');
        expect(signinSilent).toHaveBeenCalledTimes(1);
    });

    it('should retry silent login if it fails', async () => {
        vi.useFakeTimers({ toFake: ['setTimeout'] });

        const signinSilent = vi.spyOn(UserManager.prototype, 'signinSilent')
            .mockRejectedValueOnce(new Error('Timeout'))
            .mockResolvedValue(user);

        const authService = TestBed.inject(AuthService);

        const promise = authService.loginSilent();

        await vi.runAllTimersAsync();

        const profile = await promise;

        expect(profile.authorization).toEqual('Bearer token1');
        expect(signinSilent).toHaveBeenCalledTimes(2);
    });

    it('should throw error if silent login fails for all attempts', async () => {
        vi.useFakeTimers({ toFake: ['setTimeout'] });

        const signinSilent = vi.spyOn(UserManager.prototype, 'signinSilent').mockRejectedValue(new Error('Timeout'));

        const authService = TestBed.inject(AuthService);

        const promise = authService.loginSilent();
        const promiseError = expect(promise).rejects.toThrow('Retry limit exceeded.');

        await vi.runAllTimersAsync();
        await promiseError;

        expect(signinSilent).toHaveBeenCalledTimes(5);
    });
});

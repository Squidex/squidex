/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable deprecation/deprecation */

import { Location } from '@angular/common';
import { HttpErrorResponse, HttpEventType, HttpHeaders, HttpRequest } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, lastValueFrom, of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { ApiUrlConfig, AuthService } from '@app/shared/internal';
import { authInterceptor } from './auth.interceptor';

describe('AuthInterceptor', () => {
    let authService: IMock<AuthService>;
    let location = Mock.ofType<Location>();
    let router: IMock<Router>;

    beforeEach(() => {
        authService = Mock.ofType<AuthService>();
        location = Mock.ofType<Location>();
        location.setup(x => x.path()).returns(() => '/my-path');
        router = Mock.ofType<Router>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: AuthService,
                    useValue: authService.object,
                },
                {
                    provide: ApiUrlConfig,
                    useValue: new ApiUrlConfig('http://service/p/'),
                },
                {
                    provide: Location,
                    useValue: location.object,
                },
                {
                    provide: Router,
                    useValue: router.object,
                },
            ],
        });
    });

    bit('should append headers to request', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{ authorization: 'token1' }));

        const initialRequest = new HttpRequest('GET', 'http://service/p/apps', {
            headers: undefined,
        });

        const invokedRequest: HttpRequest<any>[] = [];
        await firstValueFrom(authInterceptor(initialRequest, request => {
            invokedRequest.push(request);

            return of({ type: HttpEventType.Sent });
        }));

        const req = invokedRequest[0];
        expect(invokedRequest.length).toEqual(1);
        expect(req.method).toEqual('GET');
        expect(req.headers.get('Authorization')).toEqual('token1');
        expect(req.headers.get('Accept')).toBeNull();
        expect(req.headers.get('Accept-Language')).toBeNull();
        expect(req.headers.get('Pragma')).toEqual('no-cache');
    });

    bit('should not append headers for no auth headers', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{ authToauthorizationken: 'token1' }));

        const initialRequest = new HttpRequest('GET', 'http://service/p/apps', {
            headers: new HttpHeaders().set('NoAuth', '1'),
        });

        const invokedRequest: HttpRequest<any>[] = [];
        await firstValueFrom(authInterceptor(initialRequest, request => {
            invokedRequest.push(request);

            return of({ type: HttpEventType.Sent });
        }));

        const req = invokedRequest[0];
        expect(invokedRequest.length).toEqual(1);
        expect(req.method).toEqual('GET');
        expect(req.headers.get('Authorization')).toBeNull();
        expect(req.headers.get('Accept')).toBeNull();
        expect(req.headers.get('Accept-Language')).toBeNull();
        expect(req.headers.get('Pragma')).toBeNull();
    });

    bit('should not append headers for other requests', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{ authorization: 'token1' }));

        const initialRequest = new HttpRequest('GET', 'http://cloud/p/apps', {
            headers: undefined,
        });

        const invokedRequest: HttpRequest<any>[] = [];
        await firstValueFrom(authInterceptor(initialRequest, request => {
            invokedRequest.push(request);

            return of({ type: HttpEventType.Sent });
        }));

        const req = invokedRequest[0];
        expect(invokedRequest.length).toEqual(1);
        expect(req.method).toEqual('GET');
        expect(req.headers.get('Authorization')).toBeNull();
        expect(req.headers.get('Accept')).toBeNull();
        expect(req.headers.get('Accept-Language')).toBeNull();
        expect(req.headers.get('Pragma')).toBeNull();
    });

    bit('should logout for 401 status code after retry', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{ authorization: 'token1' }));

        authService.setup(x => x.loginSilent())
            .returns(() => ofPromise(<any>{ authorization: 'token2' }));

        const initialRequest = new HttpRequest('GET', 'http://service/p/apps', {
            headers: undefined,
        });

        const invokedRequest: HttpRequest<any>[] = [];
        await lastValueFrom(authInterceptor(initialRequest, request => {
            invokedRequest.push(request);

            return throwError(() => ({ status: 401, statusText: '401' } as HttpErrorResponse));
        }).pipe(onErrorResumeNextWith()), { defaultValue: null });

        const req1 = invokedRequest[0];
        const req2 = invokedRequest[1];
        expect(invokedRequest.length).toEqual(2);
        expect(req1.headers.get('Authorization')).toEqual('token1');
        expect(req2.headers.get('Authorization')).toEqual('token2');

        authService.verify(x => x.logoutRedirect('/my-path'), Times.once());
    });

    const AUTH_ERRORS = [403];

    AUTH_ERRORS.forEach(status => {
        bit(`should redirect for ${status} status code`, async () => {
            authService.setup(x => x.userChanges)
                .returns(() => of(<any>{ authorization: 'token1' }));

            const initialRequest = new HttpRequest('GET', 'http://service/p/apps', {
                headers: undefined,
            });

            const invokedRequest: HttpRequest<any>[] = [];
            await firstValueFrom(authInterceptor(initialRequest, request => {
                invokedRequest.push(request);

                return throwError(() => ({ status } as HttpErrorResponse));
            }), { defaultValue: null });

            expect().nothing();

            router.verify(x => x.navigate(['/forbidden'], { replaceUrl: true }), Times.once());
        });
    });

    const SERVER_ERRORS = [500, 404, 405];

    SERVER_ERRORS.forEach(status => {
        bit(`should not logout for ${status} status code`, async () => {
            authService.setup(x => x.userChanges)
                .returns(() => of(<any>{ authorization: 'token1' }));

                const initialRequest = new HttpRequest('GET', 'http://service/p/apps', {
                    headers: undefined,
                });

                const invokedRequest: HttpRequest<any>[] = [];
                await firstValueFrom(authInterceptor(initialRequest, request => {
                    invokedRequest.push(request);

                    return throwError(() => ({ status } as HttpErrorResponse));
                }).pipe(onErrorResumeNextWith()), { defaultValue: null });

            expect().nothing();

            authService.verify(x => x.logoutRedirect('/my-path'), Times.never());
        });
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}

function ofPromise(value: any): Promise<any> {
    return {
        then: (onfullfilled: (value: any) => void) => {
            onfullfilled(value);
        },
    } as any;
}
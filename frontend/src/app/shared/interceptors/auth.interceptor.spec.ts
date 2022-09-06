/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable deprecation/deprecation */

import { Location } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, Mock, Times } from 'typemoq';
import { ApiUrlConfig, AuthService } from '@app/shared/internal';
import { AuthInterceptor } from './auth.interceptor';

describe('AuthInterceptor', () => {
    let authService: IMock<AuthService>;
    let location: IMock<Location>;
    let router: IMock<Router>;

    beforeEach(() => {
        location = Mock.ofType<Location>();

        location.setup(x => x.path())
            .returns(() => '/my-path');

        authService = Mock.ofType(AuthService);

        router = Mock.ofType<Router>();

        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                { provide: Router, useFactory: () => router.object },
                { provide: Location, useFactory: () => location.object },
                { provide: AuthService, useValue: authService.object },
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: AuthInterceptor,
                    multi: true,
                },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should append headers to request',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {
            authService.setup(x => x.userChanges).returns(() => of(<any>{ authorization: 'letmein' }));

            http.get('http://service/p/apps').subscribe();

            const req = httpMock.expectOne('http://service/p/apps');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('Authorization')).toEqual('letmein');
            expect(req.request.headers.get('Pragma')).toEqual('no-cache');
        }));

    it('should not append headers for no auth headers',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {
            authService.setup(x => x.userChanges).returns(() => of(<any>{ authToken: 'letmein' }));

            http.get('http://service/p/apps', { headers: new HttpHeaders().set('NoAuth', '') }).subscribe();

            const req = httpMock.expectOne('http://service/p/apps');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('Authorization')).toBeNull();
            expect(req.request.headers.get('Accept-Language')).toBeNull();
            expect(req.request.headers.get('Pragma')).toBeNull();
        }));

    it('should not append headers for other requests',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {
            authService.setup(x => x.userChanges).returns(() => of(<any>{ authToken: 'letmein' }));

            http.get('http://cloud/p/apps').subscribe();

            const req = httpMock.expectOne('http://cloud/p/apps');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('Authorization')).toBeNull();
            expect(req.request.headers.get('Accept-Language')).toBeNull();
            expect(req.request.headers.get('Pragma')).toBeNull();
        }));

    it('should logout for 401 status code',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {
            authService.setup(x => x.userChanges).returns(() => of(<any>{ authToken: 'letmein' }));
            authService.setup(x => x.loginSilent()).returns(() => of(<any>{ authToken: 'letmereallyin' }));

            http.get('http://service/p/apps').pipe(onErrorResumeNext()).subscribe();

            httpMock.expectOne('http://service/p/apps').error(<any>{}, { status: 401 });
            httpMock.expectOne('http://service/p/apps').error(<any>{}, { status: 401 });

            expect().nothing();

            authService.verify(x => x.logoutRedirect('/my-path'), Times.once());
        }));

    const AUTH_ERRORS = [403];

    AUTH_ERRORS.forEach(statusCode => {
        it(`should redirect for ${statusCode} status code`,
            inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {
                authService.setup(x => x.userChanges).returns(() => of(<any>{ authToken: 'letmein' }));

                http.get('http://service/p/apps').pipe(onErrorResumeNext()).subscribe();

                const req = httpMock.expectOne('http://service/p/apps');

                req.error(<any>{}, { status: statusCode });

                expect().nothing();

                router.verify(x => x.navigate(['/forbidden'], { replaceUrl: true }), Times.once());
            }));
    });

    const SERVER_ERRORS = [500, 404, 405];

    SERVER_ERRORS.forEach(statusCode => {
        it(`should not logout for ${statusCode} status code`,
            inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {
                authService.setup(x => x.userChanges).returns(() => of(<any>{ authToken: 'letmein' }));

                http.get('http://service/p/apps').pipe(onErrorResumeNext()).subscribe();

                const req = httpMock.expectOne('http://service/p/apps');

                req.error(<any>{}, { status: statusCode });

                expect().nothing();

                authService.verify(x => x.logoutRedirect('/my-path'), Times.never());
            }));
    });
});

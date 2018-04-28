/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HTTP_INTERCEPTORS, HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthInterceptor,
    AuthService
} from './../';

describe('AuthInterceptor', () => {
    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);

        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                { provide: AuthService, useValue: authService.object },
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: AuthInterceptor,
                    multi: true
                }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should append headers to request',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {

        authService.setup(x => x.userChanges).returns(() => Observable.of(<any>{ authToken: 'letmein' }));

        http.get('http://service/p/apps').subscribe();

        const req = httpMock.expectOne('http://service/p/apps');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('Authorization')).toEqual('letmein');
        expect(req.request.headers.get('Accept-Language')).toEqual('*');
        expect(req.request.headers.get('Pragma')).toEqual('no-cache');
    }));

    it('should not append headers for no auth headers',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {

        authService.setup(x => x.userChanges).returns(() => Observable.of(<any>{ authToken: 'letmein' }));

        http.get('http://service/p/apps', { headers: new HttpHeaders().set('NoAuth', '') }).subscribe();

        const req = httpMock.expectOne('http://service/p/apps');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('Authorization')).toBeNull();
        expect(req.request.headers.get('Accept-Language')).toBeNull();
        expect(req.request.headers.get('Pragma')).toBeNull();
    }));

    it('should not append headers for other requests',
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {

        authService.setup(x => x.userChanges).returns(() => Observable.of(<any>{ authToken: 'letmein' }));

        http.get('http://cloud/p/apps').subscribe();

        const req = httpMock.expectOne('http://cloud/p/apps');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('Authorization')).toBeNull();
        expect(req.request.headers.get('Accept-Language')).toBeNull();
        expect(req.request.headers.get('Pragma')).toBeNull();
    }));

    it(`should logout for 401 status code`,
        inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {

        authService.setup(x => x.userChanges).returns(() => Observable.of(<any>{ authToken: 'letmein' }));
        authService.setup(x => x.loginSilent()).returns(() => Observable.of(<any>{ authToken: 'letmereallyin' }));

        http.get('http://service/p/apps').onErrorResumeNext().subscribe();

        httpMock.expectOne('http://service/p/apps').error(<any>{}, { status: 401 });
        httpMock.expectOne('http://service/p/apps').error(<any>{}, { status: 401 });

        authService.verify(x => x.logoutRedirect(), Times.once());
    }));

    [403].forEach(statusCode => {
        it(`should logout for ${statusCode} status code`,
            inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {

            authService.setup(x => x.userChanges).returns(() => Observable.of(<any>{ authToken: 'letmein' }));

            http.get('http://service/p/apps').onErrorResumeNext().subscribe();

            const req = httpMock.expectOne('http://service/p/apps');

            req.error(<any>{}, { status: statusCode });

            authService.verify(x => x.logoutRedirect(), Times.once());
        }));
    });

    [500, 404, 405].forEach(statusCode => {
        it(`should not logout for ${statusCode} status code`,
            inject([HttpClient, HttpTestingController], (http: HttpClient, httpMock: HttpTestingController) => {

            authService.setup(x => x.userChanges).returns(() => Observable.of(<any>{ authToken: 'letmein' }));

            http.get('http://service/p/apps').onErrorResumeNext().subscribe();

            const req = httpMock.expectOne('http://service/p/apps');

            req.error(<any>{}, { status: statusCode });

            authService.verify(x => x.logoutRedirect(), Times.never());
        }));
    });
});
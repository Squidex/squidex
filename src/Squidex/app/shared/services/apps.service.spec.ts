/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as TypeMoq from 'typemoq';
import * as Ng2Http from '@angular/http';

import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    DateTime,
    AppCreateDto,
    AppDto,
    AppsService,
    AuthService
} from './../';

describe('AppsService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let appsService: AppsService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        appsService = new AppsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request with auth service', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            id: '123',
                            name: 'name1',
                            created: '2016-01-01',
                            lastModified: '2017-01-01'
                        }, {
                            id: '456',
                            name: 'name2',
                            created: '2016-01-01',
                            lastModified: '2017-01-01'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let apps: AppDto[] = null;
        
        appsService.getApps().subscribe(result => {
            apps = result;
        }).unsubscribe();

        expect(apps[1].id).toBe('456');
        expect(apps[1].name).toBe('name2');
        expect(apps[1].created.eq(DateTime.parseISO('2016-01-01'))).toBeTruthy();
        expect(apps[1].lastModified.eq(DateTime.parseISO('2017-01-01'))).toBeTruthy();

        authService.verifyAll();
    });

    it('should make post request', () => {
        const now = DateTime.now();

        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.is(y => y['name'] === 'new-app')))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: {
                            id: '123'
                        }
                    })
                )
            ));

        let newApp: AppDto = null;

        appsService.postApp(new AppCreateDto('new-app'), now).subscribe(result => {
            newApp = result;
        }).unsubscribe();

        expect(newApp.id).toBe('123');
        expect(newApp.name).toBe('new-app');
        expect(newApp.created.eq(now)).toBeTruthy();
        expect(newApp.lastModified.eq(now)).toBeTruthy();

        authService.verifyAll();
    });

    it('should throw fallback error on 500', () => {
        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.isAny()))
            .returns(() => Observable.throw(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        status: 500
                    })
                )
            ));

        let error = '';

        appsService.postApp(new AppCreateDto('new-app')).subscribe(x => {}, result => {
            error = result;
        }).unsubscribe();

        expect(error).toBe('A new app could not be created.');

        authService.verifyAll();
    });

    it('should throw duplicate error on 400', () => {
        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.isAny()))
            .returns(() => Observable.throw(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        status: 400
                    })
                )
            ));

        let error = '';

        appsService.postApp(new AppCreateDto('new-app')).subscribe(x => {}, result => {
            error = result;
        }).unsubscribe();

        expect(error).toBe('An app with the same name already exists.');

        authService.verifyAll();
    });
});
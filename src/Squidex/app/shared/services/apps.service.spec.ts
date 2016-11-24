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
    AppCreateDto,
    AppDto,
    AppsService,
    AuthService,
    DateTime
} from './../';

describe('AppsService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let appsService: AppsService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        appsService = new AppsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request with auth service to get apps', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            id: '123',
                            name: 'name1',
                            created: '2016-01-01',
                            lastModified: '2016-02-02',
                            permission: 'Owner'
                        }, {
                            id: '456',
                            name: 'name2',
                            created: '2017-01-01',
                            lastModified: '2017-02-02',
                            permission: 'Editor'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let apps: AppDto[] = null;
        
        appsService.getApps().subscribe(result => {
            apps = result;
        }).unsubscribe();

        expect(apps).toEqual([
            new AppDto('123', 'name1', DateTime.parseISO('2016-01-01'), DateTime.parseISO('2016-02-02'), 'Owner'),
            new AppDto('456', 'name2', DateTime.parseISO('2017-01-01'), DateTime.parseISO('2017-02-02'), 'Editor'),
        ]);

        authService.verifyAll();
    });

    it('should make post request to create app', () => {
        const createApp = new AppCreateDto('new-app');
        const now = DateTime.now();

        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.is(a => a === createApp)))
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

        appsService.postApp(createApp, now).subscribe(result => {
            newApp = result;
        }).unsubscribe();

        expect(newApp).toEqual(new AppDto('123', 'new-app', now, now, 'Owner'));

        authService.verifyAll();
    });

    it('should throw fallback error on 500 when creating app failed', () => {
        const createApp = new AppCreateDto('new-app');
        
        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.is(a => a === createApp)))
            .returns(() => Observable.throw(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        status: 500
                    })
                )
            ));

        let error = '';

        appsService.postApp(createApp).subscribe(x => {}, result => {
            error = result;
        }).unsubscribe();

        expect(error).toBe('A new app could not be created.');

        authService.verifyAll();
    });

    it('should throw duplicate error on 400 when creating app failed', () => {
        const createApp = new AppCreateDto('new-app');
        
        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.is(a => a === createApp)))
            .returns(() => Observable.throw(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        status: 400
                    })
                )
            ));

        let error = '';

        appsService.postApp(createApp).subscribe(x => {}, result => {
            error = result;
        }).unsubscribe();

        expect(error).toBe('An app with the same name already exists.');

        authService.verifyAll();
    });
});
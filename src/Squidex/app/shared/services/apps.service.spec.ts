/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2Http from '@angular/http';
import * as TypeMoq from 'typemoq';

import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    AppDto,
    AppsService,
    AuthService,
    CreateAppDto,
    DateTime,
    EntityCreatedDto
} from './../';

describe('AppsService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let appsService: AppsService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        appsService = new AppsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get apps', () => {
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
        const createApp = new CreateAppDto('new-app');

        authService.setup(x => x.authPost('http://service/p/api/apps', TypeMoq.It.isValue(createApp)))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: {
                            id: '123'
                        }
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());
            
        let newCreated: EntityCreatedDto = null;

        appsService.postApp(createApp).subscribe(result => {
            newCreated = result;
        }).unsubscribe();

        expect(newCreated).toEqual(new EntityCreatedDto('123'));

        authService.verifyAll();
    });
});
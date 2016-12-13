/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { It, Mock, Times } from 'typemoq';

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
    let authService: Mock<AuthService>;
    let appsService: AppsService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        appsService = new AppsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get apps', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
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
            .verifiable(Times.once());

        let apps: AppDto[] = null;

        appsService.getApps().subscribe(result => {
            apps = result;
        }).unsubscribe();

        expect(apps).toEqual([
            new AppDto('123', 'name1', DateTime.parseISO('2016-01-01'), DateTime.parseISO('2016-02-02'), 'Owner'),
            new AppDto('456', 'name2', DateTime.parseISO('2017-01-01'), DateTime.parseISO('2017-02-02'), 'Editor')
        ]);

        authService.verifyAll();
    });

    it('should make post request to create app', () => {
        const createApp = new CreateAppDto('new-app');

        authService.setup(x => x.authPost('http://service/p/api/apps', It.isValue(createApp)))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            id: '123'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let newCreated: EntityCreatedDto = null;

        appsService.postApp(createApp).subscribe(result => {
            newCreated = result;
        }).unsubscribe();

        expect(newCreated).toEqual(new EntityCreatedDto('123'));

        authService.verifyAll();
    });
});
/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { It, IMock, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AppContributorDto,
    AppContributorsService,
    AuthService
} from './../';

describe('AppContributorsService', () => {
    let authService: IMock<AuthService>;
    let appContributorsService: AppContributorsService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        appContributorsService = new AppContributorsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get app contributors', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/contributors'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            contributorId: '123',
                            permission: 'Owner'
                        }, {
                            contributorId: '456',
                            permission: 'Editor'
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let contributors: AppContributorDto[] = null;

        appContributorsService.getContributors('my-app').subscribe(result => {
            contributors = result;
        }).unsubscribe();

        expect(contributors).toEqual(
            [
                new AppContributorDto('123', 'Owner'),
                new AppContributorDto('456', 'Editor')
            ]);

        authService.verifyAll();
    });

    it('should make post request to assign contributor', () => {
        const contributor = new AppContributorDto('123', 'Owner');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/contributors', It.isValue(contributor)))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appContributorsService.postContributor('my-app', contributor);

        authService.verifyAll();
    });

    it('should make delete request to remove contributor', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/contributors/123'))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appContributorsService.deleteContributor('my-app', '123');

        authService.verifyAll();
    });
});
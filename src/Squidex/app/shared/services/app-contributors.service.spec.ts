/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    AuthService,
    Version
} from './../';

describe('AppContributorsService', () => {
    let authService: IMock<AuthService>;
    let appContributorsService: AppContributorsService;
    let version = new Version('1');

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        appContributorsService = new AppContributorsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get app contributors', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/contributors', version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            contributors: [
                                {
                                    contributorId: '123',
                                    permission: 'Owner'
                                },
                                {
                                    contributorId: '456',
                                    permission: 'Owner'
                                }
                            ],
                            maxContributors: 100
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let contributors: AppContributorsDto | null = null;

        appContributorsService.getContributors('my-app', version).subscribe(result => {
            contributors = result;
        }).unsubscribe();

        expect(contributors).toEqual(
            new AppContributorsDto([
                    new AppContributorDto('123', 'Owner'),
                    new AppContributorDto('456', 'Owner')
                ], 100));

        authService.verifyAll();
    });

    it('should make post request to assign contributor', () => {
        const dto = new AppContributorDto('123', 'Owner');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/contributors', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appContributorsService.postContributor('my-app', dto, version);

        authService.verifyAll();
    });

    it('should make delete request to remove contributor', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/contributors/123', version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appContributorsService.deleteContributor('my-app', '123', version);

        authService.verifyAll();
    });
});
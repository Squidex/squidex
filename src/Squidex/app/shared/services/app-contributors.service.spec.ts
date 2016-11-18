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
    AppContributor,
    AppContributorsService,
    AuthService
} from './../';

describe('AppContributorsService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let appContributorsService: AppContributorsService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        appContributorsService = new AppContributorsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request with auth service', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/contributors'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
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
            .verifiable(TypeMoq.Times.once());

        let contributors: AppContributor[] = null;
        
        appContributorsService.getContributors('my-app').subscribe(result => {
            contributors = result;
        }).unsubscribe();

        expect(contributors).toEqual(
            [
                new AppContributor('123', 'Owner'),
                new AppContributor('456', 'Editor'),
            ]);

        authService.verifyAll();
    });

    it('should make post assign contributor', () => {
        const contributor = new AppContributor('123', 'Owner');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/contributors', TypeMoq.It.is(c => c === contributor)))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions()
                )
            ));

        appContributorsService.postContributor('my-app', contributor);

        authService.verifyAll();
    });

    it('should make delete request to remove contributor', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/contributors/123'))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions()
                )
            ));

        appContributorsService.deleteContributor('my-app', '123');

        authService.verifyAll();
    });
});
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
    AppLanguagesService,
    AuthService,
    LanguageDto,
} from './../';

describe('AppLanguagesService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let appLanguagesService: AppLanguagesService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        appLanguagesService = new AppLanguagesService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request with auth service to get app languages', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/languages'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            iso2Code: 'de',
                            englishName: 'German'
                        }, {
                            iso2Code: 'en',
                            englishName: 'English'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let languages: LanguageDto[] = null;
        
        appLanguagesService.getLanguages('my-app').subscribe(result => {
            languages = result;
        }).unsubscribe();

        expect(languages).toEqual(
            [
                new LanguageDto('de', 'German'),
                new LanguageDto('en', 'English'),
            ]);

        authService.verifyAll();
    });

    it('should make post request to configure languages', () => {
        const languages = ['de', 'en'];

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/languages', TypeMoq.It.is(y => y['languages'] === languages)))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions()
                )
            ))
            .verifiable(TypeMoq.Times.once());

        appLanguagesService.postLanguages('my-app', languages);

        authService.verifyAll();
    });
});
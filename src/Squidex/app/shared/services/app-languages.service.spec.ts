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
    AppLanguageDto,
    AppLanguagesService,
    AuthService,
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
                            englishName: 'German',
                            isMasterLanguage: true
                        }, {
                            iso2Code: 'en',
                            englishName: 'English'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let languages: AppLanguageDto[] = null;
        
        appLanguagesService.getLanguages('my-app').subscribe(result => {
            languages = result;
        }).unsubscribe();

        expect(languages).toEqual(
            [
                new AppLanguageDto('de', 'German', true),
                new AppLanguageDto('en', 'English', false),
            ]);

        authService.verifyAll();
    });

    it('should make post request to add language', () => {
        const newLanguage = 'de';

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/languages', TypeMoq.It.is(y => y['language'] === newLanguage)))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: {
                            iso2Code: 'de',
                            englishName: 'German'
                        }
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let language: AppLanguageDto;

        appLanguagesService.postLanguages('my-app', newLanguage).subscribe(result => {
            language = result;
        });

        expect(language).toEqual(
            new AppLanguageDto('de', 'German', false));

        authService.verifyAll();
    });

    it('should make put request to make master language', () => {
        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/languages/de', TypeMoq.It.isValue({ isMasterLanguage: true })))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions()
                )
            ))
            .verifiable(TypeMoq.Times.once());

        appLanguagesService.makeMasterLanguage('my-app', 'de');

        authService.verifyAll();
    });

    it('should make delete request to remove language', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/languages/de'))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions()
                )
            ))
            .verifiable(TypeMoq.Times.once());

        appLanguagesService.deleteLanguage('my-app', 'de');

        authService.verifyAll();
    });
});
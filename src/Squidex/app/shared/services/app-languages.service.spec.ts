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
    AddAppLanguageDto,
    ApiUrlConfig,
    AppLanguageDto,
    AppLanguagesService,
    AuthService,
    UpdateAppLanguageDto
} from './../';

describe('AppLanguagesService', () => {
    let authService: IMock<AuthService>;
    let appLanguagesService: AppLanguagesService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        appLanguagesService = new AppLanguagesService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get app languages', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/languages'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
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
            .verifiable(Times.once());

        let languages: AppLanguageDto[] | null = null;

        appLanguagesService.getLanguages('my-app').subscribe(result => {
            languages = result;
        }).unsubscribe();

        expect(languages).toEqual(
            [
                new AppLanguageDto('de', 'German', true),
                new AppLanguageDto('en', 'English', false)
            ]);

        authService.verifyAll();
    });

    it('should make post request to add language', () => {
        const dto = new AddAppLanguageDto('de');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/languages', dto))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            iso2Code: 'de',
                            englishName: 'German'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let language: AppLanguageDto | null = null;

        appLanguagesService.postLanguages('my-app', dto).subscribe(result => {
            language = result;
        });

        expect(language).toEqual(
            new AppLanguageDto('de', 'German', false));

        authService.verifyAll();
    });

    it('should make put request to make master language', () => {
        const dto = new UpdateAppLanguageDto(true);

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/languages/de', dto))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appLanguagesService.updateLanguage('my-app', 'de', dto);

        authService.verifyAll();
    });

    it('should make delete request to remove language', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/languages/de'))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appLanguagesService.deleteLanguage('my-app', 'de');

        authService.verifyAll();
    });
});
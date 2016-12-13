/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthService,
    LanguageDto,
    LanguageService
} from './../';

describe('LanguageService', () => {
    let authService: Mock<AuthService>;
    let languageService: LanguageService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        languageService = new LanguageService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get languages', () => {
        authService.setup(x => x.authGet('http://service/p/api/languages'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
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
            .verifiable(Times.once());

        let languages: LanguageDto[] = null;

        languageService.getLanguages().subscribe(result => {
            languages = result;
        }).unsubscribe();

        expect(languages).toEqual(
            [
                new LanguageDto('de', 'German'),
                new LanguageDto('en', 'English')
            ]);

        authService.verifyAll();
    });
});
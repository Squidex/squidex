/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, LanguageDto, LanguagesService } from '@app/shared/internal';

describe('LanguageService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                LanguagesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get languages',
        inject([LanguagesService, HttpTestingController], (languagesService: LanguagesService, httpMock: HttpTestingController) => {
            let languages: ReadonlyArray<LanguageDto>;

            languagesService.getLanguages().subscribe(result => {
                languages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/languages');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([
                {
                    iso2Code: 'de',
                    englishName: 'German',
                },
                {
                    iso2Code: 'en',
                    englishName: 'English',
                },
            ]);

            expect(languages!).toEqual(
                [
                    new LanguageDto('de', 'German'),
                    new LanguageDto('en', 'English'),
                ]);
        }));
});

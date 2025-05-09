/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, TranslateDto, TranslationDto, TranslationsService } from '@app/shared/internal';

describe('TranslationsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        TranslationsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make post request to translate text',
        inject([TranslationsService, HttpTestingController], (translationsService: TranslationsService, httpMock: HttpTestingController) => {
            const dto = new TranslateDto({ text: 'Hello', sourceLanguage: 'en', targetLanguage: 'de' });

            let translation: TranslationDto;
            translationsService.translate('my-app', dto).subscribe(result => {
                translation = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/translations');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                result: 'Translated',
                status: 'Translated',
                text: 'Hallo',
            });

            expect(translation!).toEqual(new TranslationDto({
                result: 'Translated',
                status: 'Translated',
                text: 'Hallo',
            }));
        }));
});

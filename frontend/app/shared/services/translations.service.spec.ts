/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, TranslationDto, TranslationsService } from '@app/shared/internal';

describe('TranslationsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
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
            const dto = { text: 'Hello', sourceLanguage: 'en', targetLanguage: 'de' };

            let translation: TranslationDto;

            translationsService.translate('my-app', dto).subscribe(result => {
                translation = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/translations');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                text: 'Hallo', result: 'Translated',
            });

            expect(translation!).toEqual(new TranslationDto('Translated', 'Hallo'));
        }));
});

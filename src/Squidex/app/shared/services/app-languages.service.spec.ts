/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AddAppLanguageDto,
    AnalyticsService,
    ApiUrlConfig,
    AppLanguageDto,
    AppLanguagesDto,
    AppLanguagesService,
    UpdateAppLanguageDto,
    Version
} from './../';

describe('AppLanguagesService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppLanguagesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app languages',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {

        let languages: AppLanguagesDto | null = null;

        appLanguagesService.getLanguages('my-app').subscribe(result => {
            languages = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                iso2Code: 'en',
                englishName: 'English',
                isMaster: true,
                isOptional: true,
                fallback: ['de', 'en']
            },
            {
                iso2Code: 'it',
                isMaster: false,
                isOptional: false,
                englishName: 'Italian'
            }
        ], {
            headers: {
                etag: '2'
            }
        });

        expect(languages).toEqual(
            new AppLanguagesDto([
                new AppLanguageDto('en', 'English', true, true,  ['de', 'en']),
                new AppLanguageDto('it', 'Italian', false, false, [])
            ], new Version('2')));
    }));

    it('should make post request to add language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {

        const dto = new AddAppLanguageDto('de');

        let language: AppLanguageDto | null = null;

        appLanguagesService.postLanguage('my-app', dto, version).subscribe(result => {
            language = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({ iso2Code: 'de', englishName: 'German' });

        expect(language).toEqual(
            new AppLanguageDto('de', 'German', false, false, []));
    }));

    it('should make put request to make master language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {

        const dto = new UpdateAppLanguageDto(true, true, []);

        appLanguagesService.putLanguage('my-app', 'de', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages/de');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to remove language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {

        appLanguagesService.deleteLanguage('my-app', 'de', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages/de');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
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

describe('AppLanguageDto', () => {
    const language1 = new AppLanguageDto('de', 'English', false, false, []);
    const language2 = new AppLanguageDto('en', 'English', false, false, []);
    const language2_new = new AppLanguageDto('en', 'English (United States)', false, false, []);
    const version = new Version('1');
    const newVersion = new Version('2');

    it('should update languages when adding language', () => {
        const languages_1 = new AppLanguagesDto([language1], version);
        const languages_2 = languages_1.addLanguage(language2, newVersion);

        expect(languages_2.languages).toEqual([language1, language2]);
        expect(languages_2.version).toEqual(newVersion);
    });

    it('should update languages when removing language', () => {
        const languages_1 = new AppLanguagesDto([language1, language2], version);
        const languages_2 = languages_1.removeLanguage(language1, newVersion);

        expect(languages_2.languages).toEqual([language2]);
        expect(languages_2.version).toEqual(newVersion);
    });

    it('should update languages when updating language', () => {
        const languages_1 = new AppLanguagesDto([language1, language2], version);
        const languages_2 = languages_1.updateLanguage(language2_new, newVersion);

        expect(languages_2.languages).toEqual([language1, language2_new]);
        expect(languages_2.version).toEqual(newVersion);
    });
});

describe('AppLanguageDto', () => {
    it('should update properties when updating', () => {
        const language_1 = new AppLanguageDto('de', 'English', false, false, []);
        const language_2 = language_1.update(true, true, ['de', 'it']);

        expect(language_2.isMaster).toBeTruthy();
        expect(language_2.isOptional).toBeTruthy();
        expect(language_2.fallback).toEqual(['de', 'it']);
    });
});

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

        appLanguagesService.postLanguages('my-app', dto, version).subscribe(result => {
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

        appLanguagesService.updateLanguage('my-app', 'de', dto, version).subscribe();

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
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
    ApiUrlConfig,
    AppLanguageDto,
    AppLanguagesService,
    UpdateAppLanguageDto,
    Version
} from './../';


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
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app languages',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {

        let languages: AppLanguageDto[] | null = null;

        appLanguagesService.getLanguages('my-app', version).subscribe(result => {
            languages = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

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
        ]);

        expect(languages).toEqual(
            [
                new AppLanguageDto('en', 'English', true, true,  ['de', 'en']),
                new AppLanguageDto('it', 'Italian', false, false, [])
            ]);
    }));

    it('should make post request to add language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {

        const dto = new AddAppLanguageDto('de');

        let language: AppLanguageDto | null = null;

        appLanguagesService.postLanguages('my-app', dto, version).subscribe(result => {
            language = result;
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
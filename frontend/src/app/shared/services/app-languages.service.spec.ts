/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, AppLanguageDto, AppLanguagesDto, AppLanguagesPayload, AppLanguagesService, Resource, ResourceLinks, Version } from '@app/shared/internal';

describe('AppLanguagesService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                AppLanguagesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app languages',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {
            let languages: AppLanguagesDto;

            appLanguagesService.getLanguages('my-app').subscribe(result => {
                languages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(languagesResponse('en', 'de', 'it'), {
                headers: {
                    etag: '2',
                },
            });

            expect(languages!).toEqual({ payload: createLanguages('en', 'de', 'it'), version: new Version('2') });
        }));

    it('should make post request to add language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {
            const dto = { language: 'de' };

            let languages: AppLanguagesDto;

            appLanguagesService.postLanguage('my-app', dto, version).subscribe(result => {
                languages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(languagesResponse('en', 'de', 'it'), {
                headers: {
                    etag: '2',
                },
            });

            expect(languages!).toEqual({ payload: createLanguages('en', 'de', 'it'), version: new Version('2') });
        }));

    it('should make put request to make master language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {
            const dto = { isMaster: true };

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: 'api/apps/my-app/languages/de' },
                },
            };

            let languages: AppLanguagesDto;

            appLanguagesService.putLanguage('my-app', resource, dto, version).subscribe(result => {
                languages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages/de');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(languagesResponse('en', 'de', 'it'), {
                headers: {
                    etag: '2',
                },
            });

            expect(languages!).toEqual({ payload: createLanguages('en', 'de', 'it'), version: new Version('2') });
        }));

    it('should make delete request to remove language',
        inject([AppLanguagesService, HttpTestingController], (appLanguagesService: AppLanguagesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: 'api/apps/my-app/languages/de' },
                },
            };

            let languages: AppLanguagesDto;

            appLanguagesService.deleteLanguage('my-app', resource, version).subscribe(result => {
                languages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/languages/de');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(languagesResponse('en', 'de', 'it'), {
                headers: {
                    etag: '2',
                },
            });

            expect(languages!).toEqual({ payload: createLanguages('en', 'de', 'it'), version: new Version('2') });
        }));

    function languagesResponse(...codes: string[]) {
        return {
            items: codes.map((code, i) => ({
                iso2Code: code,
                englishName: code,
                isMaster: i === 0,
                isOptional: i % 2 === 1,
                fallback: codes.removed(code),
                _links: {
                    update: { method: 'PUT', href: `/languages/${code}` },
                },
            })),
            _links: {
                create: { method: 'POST', href: '/languages' },
            },
        };
    }
});

export function createLanguages(...codes: ReadonlyArray<string>): AppLanguagesPayload {
    return {
        items: codes.map((code, i) => createLanguage(code, codes, i)),
        canCreate: true,
    };
}
function createLanguage(code: string, codes: ReadonlyArray<string>, i: number) {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/languages/${code}` },
    };

    return new AppLanguageDto(links, code, code, i === 0, i % 2 === 1, codes.removed(code));
}

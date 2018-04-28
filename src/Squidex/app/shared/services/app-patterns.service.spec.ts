/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    AppPatternDto,
    AppPatternsDto,
    AppPatternsService,
    EditAppPatternDto,
    Version
} from './../';

describe('AppPatternsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppPatternsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get patterns',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

        let patterns: AppPatternsDto | null = null;

        patternService.getPatterns('my-app').subscribe(result => {
            patterns = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                patternId: '1',
                pattern: '[0-9]',
                name: 'Number',
                message: 'Message1'
            }, {
                patternId: '2',
                pattern: '[0-9]*',
                name: 'Numbers',
                message: 'Message2'
            }
        ], {
            headers: {
                etag: '2'
            }
        });

        expect(patterns).toEqual(
            new AppPatternsDto([
                new AppPatternDto('1', 'Number', '[0-9]', 'Message1'),
                new AppPatternDto('2', 'Numbers', '[0-9]*', 'Message2')
            ], new Version('2')));
    }));

    it('should make post request to add pattern',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

        const dto = new EditAppPatternDto('Number', '[0-9]', 'Message1');

        let pattern: AppPatternDto | null = null;

        patternService.postPattern('my-app', dto, version).subscribe(result => {
            pattern = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({
            patternId: '1',
            pattern: '[0-9]',
            name: 'Number',
            message: 'Message1'
        });

        expect(pattern).toEqual(new AppPatternDto('1', 'Number', '[0-9]', 'Message1'));
    }));

    it('should make put request to update pattern',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

        const dto = new EditAppPatternDto('Number', '[0-9]', 'Message1');

        patternService.putPattern('my-app', '1', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to remove pattern',
        inject([AppPatternsService, HttpTestingController], (patternService: AppPatternsService, httpMock: HttpTestingController) => {

        patternService.deletePattern('my-app', '1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});
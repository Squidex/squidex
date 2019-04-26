/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AnalyticsService,
    ApiUrlConfig,
    PatternDto,
    PatternsDto,
    PatternsService,
    Version
} from './../';

describe('PatternsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                PatternsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get patterns',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        let patterns: PatternsDto;

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

        expect(patterns!).toEqual(
            new PatternsDto([
                new PatternDto('1', 'Number', '[0-9]', 'Message1'),
                new PatternDto('2', 'Numbers', '[0-9]*', 'Message2')
            ], new Version('2')));
    }));

    it('should make post request to add pattern',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        const dto = { name: 'Number', pattern: '[0-9]' };

        let pattern: PatternDto;

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

        expect(pattern!).toEqual(new PatternDto('1', 'Number', '[0-9]', 'Message1'));
    }));

    it('should make put request to update pattern',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        const dto = { name: 'Number', pattern: '[0-9]' };

        patternService.putPattern('my-app', '1', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to remove pattern',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        patternService.deletePattern('my-app', '1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});
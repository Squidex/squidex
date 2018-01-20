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
    UpdatePatternDto,
    Version
} from './../';

describe('ApppatternsDto', () => {
    const pattern1 = new AppPatternDto('1', 'Any', '.*', 'Message1');
    const pattern2 = new AppPatternDto('2', 'Number', '[0-9]', 'Message2');
    const pattern2_new = new AppPatternDto('2', 'Numbers', '[0-9]*', 'Message2_1');
    const version = new Version('1');
    const newVersion = new Version('2');

    it('should update patterns when adding pattern', () => {
        const patterns_1 = new AppPatternsDto([pattern1], version);
        const patterns_2 = patterns_1.addPattern(pattern2, newVersion);

        expect(patterns_2.patterns).toEqual([pattern1, pattern2]);
        expect(patterns_2.version).toEqual(newVersion);
    });

    it('should update patterns when removing pattern', () => {
        const patterns_1 = new AppPatternsDto([pattern1, pattern2], version);
        const patterns_2 = patterns_1.deletePattern(pattern1, newVersion);

        expect(patterns_2.patterns).toEqual([pattern2]);
        expect(patterns_2.version).toEqual(newVersion);
    });

    it('should update patterns when updating pattern', () => {
        const patterns_1 = new AppPatternsDto([pattern1, pattern2], version);
        const patterns_2 = patterns_1.updatePattern(pattern2_new, newVersion);

        expect(patterns_2.patterns).toEqual([pattern1, pattern2_new]);
        expect(patterns_2.version).toEqual(newVersion);
    });
});

describe('AppPatternDto', () => {
    it('should update properties when updating', () => {
        const pattern_1 = new AppPatternDto('1', 'Number', '[0-9]', 'Message1');
        const pattern_2 = pattern_1.update(new UpdatePatternDto('Numbers', '[0-9]*', 'Message2'));

        expect(pattern_2.name).toBe('Numbers');
        expect(pattern_2.pattern).toBe('[0-9]*');
        expect(pattern_2.message).toBe('Message2');
    });
});

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

        const dto = new UpdatePatternDto('Number', '[0-9]', 'Message1');

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

        const dto = new UpdatePatternDto('Number', '[0-9]', 'Message1');

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
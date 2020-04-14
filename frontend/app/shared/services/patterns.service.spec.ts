/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, PatternDto, PatternsDto, PatternsPayload, PatternsService, Resource, ResourceLinks, Version } from '@app/shared/internal';

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

        req.flush(patternsResponse(1, 2, 3), {
            headers: {
                etag: '2'
            }
        });

        expect(patterns!).toEqual({payload: createPatterns(1, 2, 3), version: new Version('2') });
    }));

    it('should make post request to add pattern',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        const dto = { name: 'Number', pattern: '[0-9]' };

        let patterns: PatternsDto;

        patternService.postPattern('my-app', dto, version).subscribe(result => {
            patterns = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(patternsResponse(1, 2, 3), {
            headers: {
                etag: '2'
            }
        });

        expect(patterns!).toEqual({payload: createPatterns(1, 2, 3), version: new Version('2') });
    }));

    it('should make put request to update pattern',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        const dto = { name: 'Number', pattern: '[0-9]' };

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/apps/my-app/patterns/1' }
            }
        };

        let patterns: PatternsDto;

        patternService.putPattern('my-app', resource, dto, version).subscribe(result => {
            patterns = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(patternsResponse(1, 2, 3), {
            headers: {
                etag: '2'
            }
        });

        expect(patterns!).toEqual({payload: createPatterns(1, 2, 3), version: new Version('2') });
    }));

    it('should make delete request to remove pattern',
        inject([PatternsService, HttpTestingController], (patternService: PatternsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app/patterns/1' }
            }
        };

        let patterns: PatternsDto;

        patternService.deletePattern('my-app', resource, version).subscribe(result => {
            patterns = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/patterns/1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(patternsResponse(1, 2, 3), {
            headers: {
                etag: '2'
            }
        });

        expect(patterns!).toEqual({payload: createPatterns(1, 2, 3), version: new Version('2') });
    }));

    function patternsResponse(...ids: number[]) {
        return {
            items:  ids.map(id => ({
                id: `id${id}`,
                name: `Name${id}`,
                pattern: `Pattern${id}`,
                message: `Message${id}`,
                _links: {
                    update: { method: 'PUT', href: `/patterns/id${id}` }
                }
            })),
            _links: {
                create: { method: 'POST', href: '/patterns' }
            }
        };
    }
});

export function createPatterns(...ids: ReadonlyArray<number>): PatternsPayload {
    return {
        items: ids.map(id => createPattern(id)),
        _links: {
            create: { method: 'POST', href: '/patterns' }
        },
        canCreate: true
    };
}

export function createPattern(id: number) {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/patterns/id${id}` }
    };

    return new PatternDto(links, `id${id}`,  `Name${id}`, `Pattern${id}`, `Message${id}`);
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, IndexDto, IndexesDto, IndexesService, IndexFieldDto, Resource } from '@app/shared/internal';
import { CreateIndexDto, ResourceLinkDto } from '../model';

describe('IndexesService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
                IndexesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get indexes', inject([IndexesService, HttpTestingController], (indexesService: IndexesService, httpMock: HttpTestingController) => {
        let indexes: IndexesDto;
        indexesService.getIndexes('my-app', 'my-schema').subscribe(result => {
            indexes = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/indexes');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            items: [
                indexResponse(12),
                indexResponse(13),
            ],
            _links: {},
        });

        expect(indexes!).toEqual(new IndexesDto({
            items: [
                createIndex(12),
                createIndex(13),
            ],
            _links: {},
        }));
    }));

    it('should make post request to create index', inject([IndexesService, HttpTestingController], (indexesService: IndexesService, httpMock: HttpTestingController) => {
        const request = new CreateIndexDto({ fields: [] });

        indexesService.postIndex('my-app', 'my-schema', request).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/indexes');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make delete request to remove index', inject([IndexesService, HttpTestingController], (indexesService: IndexesService, httpMock: HttpTestingController) => {
        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app/schemas/my-schema/indexes/my-index' },
            },
        };

        indexesService.deleteIndex('my-app', resource).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/indexes/my-index');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    function indexResponse(id: number) {
        return {
            name: `index${id}`,
            fields: [
                { name: `field${id}_asc`, order: 'Ascending' },
                { name: `field${id}_desc`, order: 'Descending' },
            ],
            _links: {
                download: { method: 'GET', href: '/api/indexes/1' },
            },
        };
    }
});

export function createIndex(id: number) {
    return new IndexDto({
        name: `index${id}`,
        fields: [
            new IndexFieldDto({ name: `field${id}_asc`, order: 'Ascending' }),
            new IndexFieldDto({ name: `field${id}_desc`, order: 'Descending' }),
        ],
        _links: {
            download: new ResourceLinkDto({ method: 'GET', href: '/api/indexes/1' }),
        },
    });
}

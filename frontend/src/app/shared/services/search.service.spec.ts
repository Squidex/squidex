/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, SearchResultDto, SearchService } from '@app/shared/internal';
import { ResourceLinkDto } from './../model';

describe('SearchService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        SearchService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get search results',
        inject([SearchService, HttpTestingController], (searchService: SearchService, httpMock: HttpTestingController) => {
            let results: ReadonlyArray<SearchResultDto>;

            searchService.getResults('my-app', 'my-query').subscribe(result => {
                results = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/search?query=my-query');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([
                searchResultResponse(1),
                searchResultResponse(2),
            ]);

            expect(results!).toEqual([
                createSearchResult(1),
                createSearchResult(2),
            ]);
        }));

    function searchResultResponse(id: number) {
        return {
            name: `Search Result ${id}`,
            type: `Search Type ${id}`,
            label: `Label ${id}`,
            _links: {
                url: { method: 'GET', href: `/url${id}` },
            },
        };
    }
});

export function createSearchResult(id: number) {
    return new SearchResultDto({
        name: `Search Result ${id}`,
        type: `Search Type ${id}` as any,
        label: `Label ${id}`,
        _links: {
            url: new ResourceLinkDto({ method: 'GET', href: `/url${id}` }),
        },
    });
}

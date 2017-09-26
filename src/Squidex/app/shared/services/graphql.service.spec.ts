/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import { ApiUrlConfig, GraphQlService } from './../';

describe('GraphQlService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                GraphQlService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get history events',
        inject([GraphQlService, HttpTestingController], (graphQlService: GraphQlService, httpMock: HttpTestingController) => {

        let graphQlResult: any = null;

        graphQlService.query('my-app', { }).subscribe(result => {
            graphQlResult = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/graphql');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ result: true });

        expect(graphQlResult).toEqual({ result: true });
    }));
});
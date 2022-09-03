/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, FeaturesDto, NewsService } from '@app/shared/internal';

describe('NewsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                NewsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get features',
        inject([NewsService, HttpTestingController], (newsService: NewsService, httpMock: HttpTestingController) => {
            let features: FeaturesDto;

            newsService.getFeatures(13).subscribe(result => {
                features = result;
            });

            const req = httpMock.expectOne('http://service/p/api/news/features?version=13');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                features: [{
                    name: 'Feature1',
                    text: 'Feature Text1',
                }, {
                    name: 'Feature2',
                    text: 'Feature Text2',
                }],
                version: 13,
            });

            expect(features!).toEqual({
                features: [{
                    name: 'Feature1',
                    text: 'Feature Text1',
                }, {
                    name: 'Feature2',
                    text: 'Feature Text2',
                }],
                version: 13,
            });
        }));
});

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable deprecation/deprecation */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { StockPhotoDto, StockPhotoService } from '@app/shared/internal';

describe('StockPhotoService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                StockPhotoService,
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get stock photos',
        inject([StockPhotoService, HttpTestingController], (stockPhotoService: StockPhotoService, httpMock: HttpTestingController) => {
            let images: ReadonlyArray<StockPhotoDto>;

            stockPhotoService.getImages('my-query', 4).subscribe(result => {
                images = result;
            });

            const req = httpMock.expectOne('https://stockphoto.squidex.io/?query=my-query&page=4');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([{
                url: 'url1',
                thumbUrl: 'thumb1',
                user: 'user1',
                userProfileUrl: 'user1-url',
            }, {
                url: 'url2',
                thumbUrl: 'thumb2',
                user: 'user2',
                userProfileUrl: 'user2-url',
            }]);

            expect(images!).toEqual([
                new StockPhotoDto('url1', 'thumb1', 'user1', 'user1-url'),
                new StockPhotoDto('url2', 'thumb2', 'user2', 'user2-url'),
            ]);
        }));

    it('should return empty stock photos if get request fails',
        inject([StockPhotoService, HttpTestingController], (stockPhotoService: StockPhotoService, httpMock: HttpTestingController) => {
            let images: ReadonlyArray<StockPhotoDto>;

            stockPhotoService.getImages('my-query').subscribe(result => {
                images = result;
            });

            const req = httpMock.expectOne('https://stockphoto.squidex.io/?query=my-query&page=1');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.error(<any>{});

            expect(images!).toEqual([]);
        }));
});

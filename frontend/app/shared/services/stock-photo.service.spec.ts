/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import { StockPhotoDto, StockPhotoService } from '@app/shared/internal';

describe('StockPhotoService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                StockPhotoService
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get stock photos',
        inject([StockPhotoService, HttpTestingController], (stockPhotoService: StockPhotoService, httpMock: HttpTestingController) => {

        let images: ReadonlyArray<StockPhotoDto>;

        stockPhotoService.getImages('my-query').subscribe(result => {
            images = result;
        });

        const req = httpMock.expectOne('https://stockphoto.squidex.io/images?query=my-query&pageSize=100');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([{
            url: 'url1',
            thumbUrl: 'thumb1'
        }, {
            url: 'url2',
            thumbUrl: 'thumb2'
        }]);

        expect(images!).toEqual([
            new StockPhotoDto('url1', 'thumb1'),
            new StockPhotoDto('url2', 'thumb2')
        ]);
    }));

    it('should return empty stock photos if get request fails',
        inject([StockPhotoService, HttpTestingController], (stockPhotoService: StockPhotoService, httpMock: HttpTestingController) => {

        let images: ReadonlyArray<StockPhotoDto>;

        stockPhotoService.getImages('my-query').subscribe(result => {
            images = result;
        });

        const req = httpMock.expectOne('https://stockphoto.squidex.io/images?query=my-query&pageSize=100');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.error(<any>{});

        expect(images!).toEqual([]);
    }));
});
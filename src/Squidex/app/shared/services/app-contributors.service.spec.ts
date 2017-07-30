/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    Version
} from './../';

describe('AppContributorDto', () => {
    it('should update permission property when changing', () => {
        const contributor_1 = new AppContributorDto('1', 'Owner');
        const contributor_2 = contributor_1.changePermission('Editor');

        expect(contributor_2.permission).toBe('Editor');
    });
});

describe('AppContributorsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppContributorsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app contributors',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        let contributors: AppContributorsDto | null = null;

        appContributorsService.getContributors('my-app', version).subscribe(result => {
            contributors = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toEqual('1');

        req.flush({
            contributors: [
                {
                    contributorId: '123',
                    permission: 'Owner'
                },
                {
                    contributorId: '456',
                    permission: 'Owner'
                }
            ],
            maxContributors: 100
        });

        expect(contributors).toEqual(
            new AppContributorsDto([
                    new AppContributorDto('123', 'Owner'),
                    new AppContributorDto('456', 'Owner')
                ], 100));
    }));

    it('should make post request to assign contributor',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        const dto = new AppContributorDto('123', 'Owner');

        appContributorsService.postContributor('my-app', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual('1');

        req.flush({});
    }));

    it('should make delete request to remove contributor',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        appContributorsService.deleteContributor('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual('1');

        req.flush({});
    }));
});
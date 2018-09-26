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
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    ContributorAssignedDto,
    Version
} from './../';

describe('AppContributorsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppContributorsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app contributors',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        let contributors: AppContributorsDto;

        appContributorsService.getContributors('my-app').subscribe(result => {
            contributors = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

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
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(contributors!).toEqual(
            new AppContributorsDto([
                new AppContributorDto('123', 'Owner'),
                new AppContributorDto('456', 'Owner')
            ], 100, new Version('2')));
    }));

    it('should make post request to assign contributor',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        const dto = new AppContributorDto('123', 'Owner');

        let contributorAssignedDto: ContributorAssignedDto;

        appContributorsService.postContributor('my-app', dto, version).subscribe(result => {
            contributorAssignedDto = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({ contributorId: '123' });

        expect(contributorAssignedDto!.contributorId).toEqual('123');
    }));

    it('should make delete request to remove contributor',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        appContributorsService.deleteContributor('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});
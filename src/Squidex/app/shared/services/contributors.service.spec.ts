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
    ContributorAssignedDto,
    ContributorDto,
    ContributorsDto,
    ContributorsService,
    Version
} from '@app/shared/internal';

describe('ContributorsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                ContributorsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app contributors',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {

        let contributors: ContributorsDto;

        contributorsService.getContributors('my-app').subscribe(result => {
            contributors = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            contributors: [
                {
                    contributorId: '123',
                    role: 'Owner'
                },
                {
                    contributorId: '456',
                    role: 'Owner'
                }
            ],
            maxContributors: 100
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(contributors!).toEqual({
            payload: {
                contributors: [
                    new ContributorDto('123', 'Owner'),
                    new ContributorDto('456', 'Owner')
                ],
                maxContributors: 100
            },
            version: new Version('2')
        });
    }));

    it('should make post request to assign contributor',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {

        const dto = { contributorId: '123', role: 'Owner' };

        let contributorAssignedDto: ContributorAssignedDto;

        contributorsService.postContributor('my-app', dto, version).subscribe(result => {
            contributorAssignedDto = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({ contributorId: '123', isCreated: true });

        expect(contributorAssignedDto!.contributorId).toEqual('123');
    }));

    it('should make delete request to remove contributor',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {

        contributorsService.deleteContributor('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});
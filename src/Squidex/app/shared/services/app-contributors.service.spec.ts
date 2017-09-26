/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AnalyticsService,
    ApiUrlConfig,
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    Version
} from './../';

describe('AppContributorsDto', () => {
    const contributor1 = new AppContributorDto('1', 'Owner');
    const contributor2 = new AppContributorDto('2', 'Developer');
    const contributor2_new = new AppContributorDto('2', 'Editor');
    const version = new Version('1');
    const newVersion = new Version('2');

    it('should update contributors when adding contributor', () => {
        const contributors_1 = new AppContributorsDto([contributor1], 4, version);
        const contributors_2 = contributors_1.addContributor(contributor2, newVersion);

        expect(contributors_2.contributors).toEqual([contributor1, contributor2]);
        expect(contributors_2.version).toEqual(newVersion);
    });

    it('should update contributors when removing contributor', () => {
        const contributors_1 = new AppContributorsDto([contributor1, contributor2], 4, version);
        const contributors_2 = contributors_1.removeContributor(contributor1, newVersion);

        expect(contributors_2.contributors).toEqual([contributor2]);
        expect(contributors_2.version).toEqual(newVersion);
    });

    it('should update contributors when updating contributor', () => {
        const contributors_1 = new AppContributorsDto([contributor1, contributor2], 4, version);
        const contributors_2 = contributors_1.updateContributor(contributor2_new, newVersion);

        expect(contributors_2.contributors).toEqual([contributor1, contributor2_new]);
        expect(contributors_2.version).toEqual(newVersion);
    });
});

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

        let contributors: AppContributorsDto | null = null;

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

        expect(contributors).toEqual(
            new AppContributorsDto([
                new AppContributorDto('123', 'Owner'),
                new AppContributorDto('456', 'Owner')
            ], 100, new Version('2')));
    }));

    it('should make post request to assign contributor',
        inject([AppContributorsService, HttpTestingController], (appContributorsService: AppContributorsService, httpMock: HttpTestingController) => {

        const dto = new AppContributorDto('123', 'Owner');

        appContributorsService.postContributor('my-app', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
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
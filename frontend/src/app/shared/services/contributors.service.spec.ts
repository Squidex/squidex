/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, ContributorDto, ContributorsDto, ContributorsPayload, ContributorsService, Resource, ResourceLinks, Version } from '@app/shared/internal';

describe('ContributorsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                ContributorsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() },
            ],
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

            req.flush(contributorsResponse(1, 2, 3), {
                headers: {
                    etag: '2',
                },
            });

            expect(contributors!).toEqual({ payload: createContributors(1, 2, 3), version: new Version('2') });
        }));

    it('should make post request to assign contributor',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {
            const dto = { contributorId: '123', role: 'Owner' };

            let contributors: ContributorsDto;

            contributorsService.postContributor('my-app', dto, version).subscribe(result => {
                contributors = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(contributorsResponse(1, 2, 3), {
                headers: {
                    etag: '2',
                },
            });

            expect(contributors!).toEqual({ payload: createContributors(1, 2, 3), version: new Version('2') });
        }));

    it('should make delete request to remove contributor',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/contributors/123' },
                },
            };

            let contributors: ContributorsDto;

            contributorsService.deleteContributor('my-app', resource, version).subscribe(result => {
                contributors = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors/123');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(contributorsResponse(1, 2, 3), {
                headers: {
                    etag: '2',
                },
            });

            expect(contributors!).toEqual({ payload: createContributors(1, 2, 3), version: new Version('2') });
        }));

    function contributorsResponse(...ids: number[]) {
        return {
            items: ids.map(id => ({
                contributorId: `id${id}`,
                contributorName: `name${id}`,
                contributorEmail: `mail${id}@squidex.io`,
                role: id % 2 === 0 ? 'Owner' : 'Developer',
                _links: {
                    update: { method: 'PUT', href: `/contributors/id${id}` },
                },
            })),
            maxContributors: ids.length * 13,
            _links: {
                create: { method: 'POST', href: '/contributors' },
            },
            _meta: {
                isInvited: 'true',
            },
        };
    }
});

export function createContributors(...ids: ReadonlyArray<number>): ContributorsPayload {
    return {
        items: ids.map(createContributor),
        maxContributors: ids.length * 13,
        _links: {
            create: { method: 'POST', href: '/contributors' },
        },
        _meta: {
            isInvited: 'true',
        },
        canCreate: true,
    };
}

export function createContributor(id: number) {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/contributors/id${id}` },
    };

    return new ContributorDto(links, `id${id}`, `name${id}`, `mail${id}@squidex.io`, id % 2 === 0 ? 'Owner' : 'Developer');
}

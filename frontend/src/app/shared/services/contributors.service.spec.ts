/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, ContributorDto, ContributorsDto, ContributorsService, Resource, Versioned, VersionTag } from '@app/shared/internal';
import { ContributorsMetadataDto, ResourceLinkDto } from '../model';

describe('ContributorsService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        ContributorsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app contributors',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {
            let contributors: Versioned<ContributorsDto>;
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

            expect(contributors!).toEqual({ payload: createContributors(1, 2, 3), version: new VersionTag('2') });
        }));

    it('should make post request to assign contributor',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {
            const dto = { contributorId: '123', role: 'Owner' };

            let contributors: Versioned<ContributorsDto>;
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

            expect(contributors!).toEqual({ payload: createContributors(1, 2, 3), version: new VersionTag('2') });
        }));

    it('should make delete request to remove contributor',
        inject([ContributorsService, HttpTestingController], (contributorsService: ContributorsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/contributors/123' },
                },
            };

            let contributors: Versioned<ContributorsDto>;
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

            expect(contributors!).toEqual({ payload: createContributors(1, 2, 3), version: new VersionTag('2') });
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

export function createContributors(...ids: ReadonlyArray<number>) {
    return new ContributorsDto({
        items: ids.map(createContributor),
        maxContributors: ids.length * 13,
        _links: {
            create: new ResourceLinkDto({ method: 'POST', href: '/contributors' }),
        },
        _meta: new ContributorsMetadataDto({
            isInvited: 'true',
        }),
    });
}

export function createContributor(id: number) {
    return new ContributorDto({
        contributorId: `id${id}`,
        contributorName: `name${id}`,
        contributorEmail: `mail${id}@squidex.io`,
        role: id % 2 === 0 ? 'Owner' : 'Developer',
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/contributors/id${id}` }),
        },
    });
}

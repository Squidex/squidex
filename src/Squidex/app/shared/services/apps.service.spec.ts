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
    AppDto,
    AppsService,
    DateTime,
    Resource,
    ResourceLinks,
    Version
} from '@app/shared/internal';

describe('AppsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get apps',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {

        let apps: AppDto[];

        appsService.getApps().subscribe(result => {
            apps = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            appResponse(12),
            appResponse(13)
        ]);

        expect(apps!).toEqual([createApp(12), createApp(13)]);
    }));

    it('should make post request to create app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {

        const dto = { name: 'new-app' };

        let app: AppDto;

        appsService.postApp(dto).subscribe(result => {
            app = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(appResponse(12));

        expect(app!).toEqual(createApp(12));
    }));

    it('should make put request to update app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/apps/my-app' }
            }
        };

        let app: AppDto;

        appsService.putApp(resource, { }, version).subscribe(result => {
            app = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(appResponse(12));

        expect(app!).toEqual(createApp(12));
    }));

    it('should make delete request to archive app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app' }
            }
        };

        appsService.deleteApp(resource).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    function appResponse(id: number, suffix = '') {
        return {
            id: `id${id}`,
            name: `my-name${id}${suffix}`,
            label: `my-label${id}${suffix}`,
            description: `my-description${id}${suffix}`,
            permissions: ['Owner'],
            created: `${id % 1000 + 2000}-12-12T10:10:00`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00`,
            canAccessApi: id % 2 === 0,
            canAccessContent: id % 2 === 0,
            planName: 'Free',
            planUpgrade: 'Basic',
            version: id,
            _links: {
                schemas: { method: 'GET', href: '/schemas' }
            }
        };
    }
});

export function createApp(id: number, suffix = '') {
    const links: ResourceLinks = {
        schemas: { method: 'GET', href: '/schemas' }
    };

    return new AppDto(links,
        `id${id}`,
        `my-name${id}${suffix}`,
        `my-label${id}${suffix}`,
        `my-description${id}${suffix}`,
        ['Owner'],
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-12-12T10:10:00`),
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-11-11T10:10:00`),
        id % 2 === 0,
        id % 2 === 0,
        'Free', 'Basic',
        new Version(`${id}`));
}
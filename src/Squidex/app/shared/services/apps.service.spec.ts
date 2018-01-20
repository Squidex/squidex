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
    CreateAppDto,
    DateTime
} from './../';

describe('AppsService', () => {
    const now = DateTime.now();

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

        let apps: AppDto[] | null = null;

        appsService.getApps().subscribe(result => {
            apps = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: '123',
                name: 'name1',
                permission: 'Owner',
                created: '2016-01-01',
                lastModified: '2016-02-02',
                planName: 'Free',
                planUpgrade: 'Basic'
            },
            {
                id: '456',
                name: 'name2',
                permission: 'Owner',
                created: '2017-01-01',
                lastModified: '2017-02-02',
                planName: 'Basic',
                planUpgrade: 'Enterprise'
            }
        ]);

        expect(apps).toEqual([
            new AppDto('123', 'name1', 'Owner', DateTime.parseISO('2016-01-01'), DateTime.parseISO('2016-02-02'), 'Free', 'Basic'),
            new AppDto('456', 'name2', 'Owner', DateTime.parseISO('2017-01-01'), DateTime.parseISO('2017-02-02'), 'Basic', 'Enterprise')
        ]);
    }));

    it('should make post request to create app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {

        const dto = new CreateAppDto('new-app');

        let app: AppDto | null = null;

        appsService.postApp(dto, now).subscribe(result => {
            app = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: '123',
            permission: 'Reader',
            planName: 'Basic',
            planUpgrade: 'Enterprise'
        });

        expect(app).toEqual(new AppDto('123', dto.name, 'Reader', now, now, 'Basic', 'Enterprise'));
    }));
});
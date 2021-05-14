/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, BackupDto, BackupsDto, BackupsService, DateTime, Resource, ResourceLinks, RestoreDto } from '@app/shared/internal';

describe('BackupsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                BackupsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get backups',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            let backups: BackupsDto;

            backupsService.getBackups('my-app').subscribe(result => {
                backups = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/backups');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                items: [
                    backupResponse(12),
                    backupResponse(13),
                ],
            });

            expect(backups!).toEqual(
                new BackupsDto(2, [
                    createBackup(12),
                    createBackup(13),
                ], {}));
        }));

    it('should make get request to get restore',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            let restore: RestoreDto;

            backupsService.getRestore().subscribe(result => {
                restore = result!;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                url: 'http://url',
                started: '2017-02-03',
                stopped: '2017-02-04',
                status: 'Failed',
                log: [
                    'log1',
                    'log2',
                ],
            });

            expect(restore!).toEqual(
                new RestoreDto('http://url',
                    DateTime.parseISO('2017-02-03'),
                    DateTime.parseISO('2017-02-04'),
                    'Failed',
                    [
                        'log1',
                        'log2',
                    ]));
        }));

    it('should return null if get restore return 404',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            let restore: RestoreDto | null;

            backupsService.getRestore().subscribe(result => {
                restore = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({}, { status: 404, statusText: '404' });

            expect(restore!).toBeNull();
        }));

    it('should throw error if get restore return non 404',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            let restore: RestoreDto | null;
            let error: any;

            backupsService.getRestore().subscribe(result => {
                restore = result;
            }, err => {
                error = err;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({}, { status: 500, statusText: '500' });

            expect(restore!).toBeUndefined();
            expect(error)!.toBeDefined();
        }));

    it('should make post request to start backup',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            backupsService.postBackup('my-app').subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/backups');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make post request to start restore',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            const dto = { url: 'http://url' };

            backupsService.postRestore(dto).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to remove language',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/backups/1' },
                },
            };

            backupsService.deleteBackup('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/backups/1');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    function backupResponse(id: number) {
        return {
            id: `id${id}`,
            started: `${id % 1000 + 2000}-12-12T10:10:00Z`,
            stopped: id % 2 === 0 ? `${id % 1000 + 2000}-11-11T10:10:00Z` : null,
            handledEvents: id * 17,
            handledAssets: id * 23,
            status: id % 2 === 0 ? 'Success' : 'Failed',
            _links: {
                download: { method: 'GET', href: '/api/backups/1' },
            },
        };
    }
});

export function createBackup(id: number) {
    const links: ResourceLinks = {
        download: { method: 'GET', href: '/api/backups/1' },
    };

    return new BackupDto(links,
        `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`),
        id % 2 === 0 ? DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`) : null,
        id * 17,
        id * 23,
        id % 2 === 0 ? 'Success' : 'Failed');
}

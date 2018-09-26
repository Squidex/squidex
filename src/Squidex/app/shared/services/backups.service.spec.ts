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
    BackupDto,
    BackupsService,
    DateTime,
    RestoreDto,
    StartRestoreDto
} from './../';

describe('BackupsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                BackupsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get backups',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {

        let backups: BackupDto[];

        backupsService.getBackups('my-app').subscribe(result => {
            backups = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/backups');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: '1',
                started: '2017-02-03',
                stopped: '2017-02-04',
                handledEvents: 13,
                handledAssets: 17,
                status: 'Failed'
            },
            {
                id: '2',
                started: '2018-02-03',
                stopped: null,
                handledEvents: 23,
                handledAssets: 27,
                status: 'Completed'
            }
        ]);

        expect(backups!).toEqual(
            [
                new BackupDto('1', DateTime.parseISO_UTC('2017-02-03'), DateTime.parseISO_UTC('2017-02-04'), 13, 17, 'Failed'),
                new BackupDto('2', DateTime.parseISO_UTC('2018-02-03'), null, 23, 27, 'Completed')
            ]);
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
                'log2'
            ]
        });

        expect(restore!).toEqual(
            new RestoreDto('http://url',
                DateTime.parseISO_UTC('2017-02-03'),
                DateTime.parseISO_UTC('2017-02-04'),
                'Failed',
                [
                    'log1',
                    'log2'
                ]));
    }));

    it('should return null when get restore return 404',
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

    it('should throw error when get restore return non 404',
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

        backupsService.postRestore(new StartRestoreDto('http://url')).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/restore');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make delete request to remove language',
        inject([BackupsService, HttpTestingController], (backupsService: BackupsService, httpMock: HttpTestingController) => {

        backupsService.deleteBackup('my-app', '1').subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/backups/1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));
});
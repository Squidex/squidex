/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, DateTime, ScriptLogDto, ScriptLogEntryDto, ScriptLogsDto, ScriptLogsService } from '@app/shared/internal';

describe('ScriptLogsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
                ScriptLogsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get script logs', inject([ScriptLogsService, HttpTestingController], (scriptLogsService: ScriptLogsService, httpMock: HttpTestingController) => {
        let logs: ScriptLogsDto;
        scriptLogsService.getScriptLogs('my-app').subscribe(result => {
            logs = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/script-logs?take=100');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            items: [
                scriptLogResponse(12),
                scriptLogResponse(13),
            ],
        });

        expect(logs!).toEqual(new ScriptLogsDto({
            items: [
                createScriptLog(12),
                createScriptLog(13),
            ],
        }));
    }));

    it('should make get request to get script logs with name filter', inject([ScriptLogsService, HttpTestingController], (scriptLogsService: ScriptLogsService, httpMock: HttpTestingController) => {
        scriptLogsService.getScriptLogs('my-app', 'contents/my schema').subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/script-logs?take=100&name=contents%2Fmy%20schema');

        expect(req.request.method).toEqual('GET');

        req.flush({ items: [] });
    }));

    function scriptLogResponse(id: number) {
        return {
            id: `id${id}`,
            name: `contents/schema${id}/create`,
            timestamp: `${2017 + id}-12-12T10:10:00Z`,
            entries: [
                { timestamp: `${2017 + id}-12-12T10:10:01Z`, level: 'log', message: `Message${id}` },
            ],
            totalEntries: id,
        };
    }
});

export function createScriptLog(id: number) {
    return new ScriptLogDto({
        id: `id${id}`,
        name: `contents/schema${id}/create`,
        timestamp: DateTime.parseISO(`${2017 + id}-12-12T10:10:00Z`),
        entries: [
            new ScriptLogEntryDto({ timestamp: DateTime.parseISO(`${2017 + id}-12-12T10:10:01Z`), level: 'log', message: `Message${id}` }),
        ],
        totalEntries: id,
    });
}

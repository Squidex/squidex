/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    UIService,
    UISettingsDto
} from './../';

describe('UIService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                UIService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get settings',
        inject([UIService, HttpTestingController], (uiService: UIService, httpMock: HttpTestingController) => {

        let settings1: UISettingsDto | null = null;
        let settings2: UISettingsDto | null = null;

        uiService.getSettings().subscribe(result => {
            settings1 = result;
        });

        const response: UISettingsDto = { regexSuggestions: [] };

        const req = httpMock.expectOne('http://service/p/api/ui/settings');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(response);

        uiService.getSettings().subscribe(result => {
            settings2 = result;
        });

        expect(settings1).toEqual(response);
        expect(settings2).toEqual(response);
    }));

    it('should return default settings when error occurs',
        inject([UIService, HttpTestingController], (uiService: UIService, httpMock: HttpTestingController) => {

        let settings: UISettingsDto | null = null;

        uiService.getSettings().subscribe(result => {
            settings = result;
        });

        const req = httpMock.expectOne('http://service/p/api/ui/settings');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.error(new ErrorEvent('500'));

        expect(settings.regexSuggestions).toEqual([]);
    }));
});
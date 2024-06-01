/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable deprecation/deprecation */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { HelpService, SDKEntry } from '@app/shared/internal';

describe('HelpService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        HelpService,
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get help sections',
        inject([HelpService, HttpTestingController], (helpService: HelpService, httpMock: HttpTestingController) => {
            let helpSections: string;

            helpService.getHelp('01-chapter/02-article').subscribe(result => {
                helpSections = result;
            });

            const req = httpMock.expectOne('https://raw.githubusercontent.com/squidex/squidex-docs2/master/01-chapter/02-article.md');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush('Markdown');

            expect(helpSections!).toEqual('Markdown');
        }));

    it('should return empty sections if get request fails',
        inject([HelpService, HttpTestingController], (helpService: HelpService, httpMock: HttpTestingController) => {
            let helpSections: string;

            helpService.getHelp('01-chapter/02-article').subscribe(result => {
                helpSections = result;
            });

            const req = httpMock.expectOne('https://raw.githubusercontent.com/squidex/squidex-docs2/master/01-chapter/02-article.md');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.error(<any>{});

            expect(helpSections!).toEqual('');
        }));

    it('should make get request to get sdks',
        inject([HelpService, HttpTestingController], (helpService: HelpService, httpMock: HttpTestingController) => {
            let sdks: Record<string, SDKEntry>;

            helpService.getSDKs().subscribe(result => {
                sdks = result;
            });

            const req = httpMock.expectOne('https://raw.githubusercontent.com/Squidex/sdk-fern/main/sdks.json');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                dotnet: {},
            });

            expect(sdks!).toEqual({
                dotnet: {} as any,
            });
        }));

    it('should return empty sdks if get request fails',
        inject([HelpService, HttpTestingController], (helpService: HelpService, httpMock: HttpTestingController) => {
            let sdks: Record<string, SDKEntry>;

            helpService.getSDKs().subscribe(result => {
                sdks = result;
            });

            const req = httpMock.expectOne('https://raw.githubusercontent.com/Squidex/sdk-fern/main/sdks.json');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.error(<any>{});

            expect(sdks!).toEqual({});
        }));
});

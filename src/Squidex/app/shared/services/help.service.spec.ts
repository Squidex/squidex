/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import { HelpService } from './../';

describe('AppClientsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                HelpService
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get help sections',
        inject([HelpService, HttpTestingController], (helpService: HelpService, httpMock: HttpTestingController) => {

        let helpSections: string[] | null = null;

        helpService.getHelp('01-chapter/02-article').subscribe(result => {
            helpSections = result;
        });

        const req = httpMock.expectOne('https://api.gitbook.com/book/squidex/squidex/contents/01-chapter/02-article.json');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            sections: [
                {
                    content: 'A test content with'
                },
                {
                    content: 'A test content with a <a href="https://squidex.io">A Link</a>'
                },
                {
                    content: 'A test content with a <a href="../GLOSSARY.html#content">Glossary Link</a>'
                }
            ]
        });

        expect(helpSections).toEqual([
            'A test content with',
            'A test content with a <a href="https://squidex.io">A Link</a>',
            'A test content with a <a target="_blank" href="https://docs.squidex.io/GLOSSARY.html#content">Glossary Link</a>'
        ]);
    }));

    it('should return empty sections if get request fails',
        inject([HelpService, HttpTestingController], (helpService: HelpService, httpMock: HttpTestingController) => {

        let helpSections: string[] | null = null;

        helpService.getHelp('01-chapter/02-article').subscribe(result => {
            helpSections = result;
        });

        const req = httpMock.expectOne('https://api.gitbook.com/book/squidex/squidex/contents/01-chapter/02-article.json');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.error(<any>{});

        expect(helpSections).toEqual([]);
    }));
});
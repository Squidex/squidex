/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Http, Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { HelpService } from './../';

describe('AppClientsService', () => {
    let helpService: HelpService;
    let http: IMock<Http>;

    beforeEach(() => {
        http = Mock.ofType(Http);

        helpService = new HelpService(http.object);
    });

    it('should make get request to get help sections', () => {
        http.setup(x => x.get('https://api.gitbook.com/book/squidex/squidex/contents/01-chapter/02-article.json'))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
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
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let helpSections: string[] | null = null;

        helpService.getHelp('01-chapter/02-article').subscribe(result => {
            helpSections = result;
        });

        expect(helpSections).toEqual([
            'A test content with',
            'A test content with a <a href="https://squidex.io">A Link</a>',
            'A test content with a <a target="_blank" href="https://docs.squidex.io/GLOSSARY.html#content">Glossary Link</a>'
        ]);

        http.verifyAll();
    });

    it('should return empty sections if get request fails', () => {
        http.setup(x => x.get('https://api.gitbook.com/book/squidex/squidex/contents/01-chapter/02-article.json'))
            .returns(() => Observable.throw('An error'))
            .verifiable(Times.once());

        let helpSections: string[] | null = null;

        helpService.getHelp('01-chapter/02-article').subscribe(result => {
            helpSections = result;
        });

        expect(helpSections).toEqual([]);

        http.verifyAll();
    });
});
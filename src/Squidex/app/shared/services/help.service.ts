/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs';

@Injectable()
export class HelpService {
    constructor(
        private readonly http: Http
    ) {
    }

    public getHelp(helpPage: string): Observable<string[]> {
        const url = `https://api.gitbook.com/book/squidex/squidex/contents/${helpPage}.json`;

        return this.http.get(url)
            .map(response => response.json())
            .map(response => {
                const result: string[] = [];

                for (let section of response.sections) {
                    const content = section.content.replace(/href="\.\.\/GLOSSARY\.html/g, 'target="_blank" href="https://docs.squidex.io/GLOSSARY.html');

                    result.push(content);
                }

                return result;
            })
            .catch(err => Observable.of([]));
    }
}
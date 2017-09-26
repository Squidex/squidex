/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class HelpService {
    constructor(
        private readonly http: HttpClient
    ) {
    }

    public getHelp(helpPage: string): Observable<string[]> {
        const url = `https://api.gitbook.com/book/squidex/squidex/contents/${helpPage}.json`;

        return this.http.get(url)
            .map((response: any) => {
                const result: string[] = [];

                for (let section of response.sections) {
                    const content = section.content.replace(/href="\.\.\/GLOSSARY\.html/g, 'target="_blank" href="https://docs.squidex.io/GLOSSARY.html');

                    result.push(content);
                }

                return result;
            })
            .catch(error => Observable.of([]));
    }
}
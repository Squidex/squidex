/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class HelpService {
    constructor(
        private readonly http: HttpClient
    ) {
    }

    public getHelp(helpPage: string): Observable<string> {
        const url = `https://raw.githubusercontent.com/Squidex/squidex-docs/master/${helpPage}.md`;

        return this.http.get(url, { responseType: 'text' }).pipe(
            catchError(() => of('')));
    }
}
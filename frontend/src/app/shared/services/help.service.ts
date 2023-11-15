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

export interface SDKEntry {
    // The display name.
    name: string;

    // The link to the repository.
    repository: string;

    // The link to the documentation.
    documentation: string;

    // The instructions as markdown.
    instructions: string;

    // The SVG logo.
    logo: string;
}

@Injectable({
    providedIn: 'root',
})
export class HelpService {
    constructor(
        private readonly http: HttpClient,
    ) {
    }

    public getHelp(helpPage: string): Observable<string> {
        const url = `https://raw.githubusercontent.com/squidex/squidex-docs2/master/${helpPage}.md`;

        return this.http.get(url, { responseType: 'text' }).pipe(
            catchError(() => of('')));
    }

    public getSDKs(): Observable<Record<string, SDKEntry>> {
        const url = 'https://raw.githubusercontent.com/Squidex/sdk-fern/main/sdks.json';

        return this.http.get<Record<string, SDKEntry>>(url).pipe(
            catchError(() => of({})));
    }
}

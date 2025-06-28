/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, pretifyError, Resource } from '@app/framework';
import { TemplateDetailsDto, TemplatesDto } from './../model';

@Injectable({
    providedIn: 'root',
})
export class TemplatesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getTemplates(): Observable<TemplatesDto> {
        const url = this.apiUrl.buildUrl('api/templates?includeDetails=true');

        return this.http.get<any>(url).pipe(
            map(body => {
                return TemplatesDto.fromJSON(body);
            }),
            pretifyError('i18n:templates.loadFailed'));
    }

    public getTemplate(resource: Resource): Observable<TemplateDetailsDto> {
        const link = resource._links['self'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.get<any>(url).pipe(
            map(body => {
                return TemplateDetailsDto.fromJSON(body);
            }),
            pretifyError('i18n:templates.loadFailed'));
    }
}
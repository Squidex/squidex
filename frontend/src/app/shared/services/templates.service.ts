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
import { ApiUrlConfig, pretifyError, Resource, ResourceLinks, ResultSet } from '@app/framework';

export class TemplatesDto extends ResultSet<TemplateDto> {
}

export class TemplateDto {
    public readonly _links: ResourceLinks;
    
    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly title: string,
        public readonly description: string,
        public readonly isStarter: boolean,
    ) {
        this._links = links;
    }
}

export class TemplateDetailsDto {
    public readonly _links: ResourceLinks;

    constructor(links: ResourceLinks,
        public readonly details: string,
    ) {
        this._links = links;
    }
}

@Injectable()
export class TemplatesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getTemplates(): Observable<TemplatesDto> {
        const url = this.apiUrl.buildUrl('api/templates');

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseTemplates(body);
            }),
            pretifyError('i18n:templates.loadFailed'));
    }

    public getTemplate(resource: Resource): Observable<TemplateDetailsDto> {
        const link = resource._links['self'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseTemplateDetails(body);
            }),
            pretifyError('i18n:templates.loadFailed'));
    }
}

function parseTemplates(response: { items: any[] } & Resource) {
    const items = response.items.map(item => 
        new TemplateDto(item._links,
            item.name,
            item.title,
            item.description,
            item.isStarter));

    return new TemplatesDto(items.length, items, response._links);
}

function parseTemplateDetails(response: any & Resource) {
    return new TemplateDetailsDto(response._links,
        response.details);
}
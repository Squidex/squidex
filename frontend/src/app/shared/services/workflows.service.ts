/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/framework';
import { AddWorkflowDto, UpdateWorkflowDto, WorkflowsDto } from '../model';

@Injectable({
    providedIn: 'root',
})
export class WorkflowsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getWorkflows(appName: string): Observable<Versioned<WorkflowsDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/workflows`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return WorkflowsDto.fromJSON(body);
            }),
            pretifyError('i18n:workflows.loadFailed'));
    }

    public postWorkflow(appName: string, dto: AddWorkflowDto, version: VersionOrTag): Observable<Versioned<WorkflowsDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/workflows`);

        return HTTP.postVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return WorkflowsDto.fromJSON(body);
            }),
            pretifyError('i18n:workflows.createFailed'));
    }

    public putWorkflow(appName: string, resource: Resource, dto: UpdateWorkflowDto, version: VersionOrTag): Observable<Versioned<WorkflowsDto>> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            mapVersioned(({ body }) => {
                return WorkflowsDto.fromJSON(body);
            }),
            pretifyError('i18n:workflows.updateFailed'));
    }

    public deleteWorkflow(appName: string, resource: Resource, version: VersionOrTag): Observable<Versioned<WorkflowsDto>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return WorkflowsDto.fromJSON(body);
            }),
            pretifyError('i18n:workflows.deleteFailed'));
    }
}
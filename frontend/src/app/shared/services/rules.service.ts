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
import { ApiUrlConfig, HTTP, pretifyError, Resource, ScriptCompletions, StringHelper, VersionOrTag } from '@app/framework';
import { DynamicCreateRuleDto, DynamicRuleDto, DynamicRulesDto, DynamicUpdateRuleDto, RuleElementDto, RuleEventsDto, SimulatedRuleEventsDto } from './../model';

export type RuleTriggerMetadataDto = Readonly<{
    description: string;
    display: string;
    iconColor?: string;
    iconCode?: string | null;
    iconImage?: string;
    title?: string;
    readMore?: string;
}>;

export const ALL_TRIGGERS: Record<string, RuleTriggerMetadataDto> = {
    AssetChanged: {
        description: 'For asset changes like uploaded, updated (reuploaded), renamed, deleted...',
        display: 'Asset changed',
        iconColor: '#3389ff',
        iconCode: 'assets',
        title: 'Asset changed',
    },
    Comment: {
        description: 'When a user is mentioned in any comment...',
        display: 'User mentioned',
        iconColor: '#3389ff',
        iconCode: 'comments',
        title: 'User mentioned',
    },
    ContentChanged: {
        description: 'For content changes like created, updated, published, unpublished...',
        display: 'Content changed',
        iconColor: '#3389ff',
        iconCode: 'contents',
        title: 'Content changed',
    },
    Manual: {
        description: 'To invoke processes manually, for example to update your static site...',
        display: 'Manually triggered',
        iconColor: '#3389ff',
        iconCode: 'play-line',
        title: 'Manually triggered',
    },
    SchemaChanged: {
        description: 'When a schema definition has been created, updated, published or deleted...',
        display: 'Schema changed',
        iconColor: '#3389ff',
        iconCode: 'schemas',
        title: 'Schema changed',
    },
    Usage: {
        description: 'When monthly API calls exceed a specified limit for one time a month...',
        display: 'Usage exceeded',
        iconColor: '#3389ff',
        iconCode: 'dashboard',
        title: 'Usage',
    },
};

export type StepsDto = Readonly<{ [name: string]: RuleElementDto }>;

@Injectable({
    providedIn: 'root',
})
export class RulesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getSteps(): Observable<StepsDto> {
        const url = this.apiUrl.buildUrl('api/rules/steps');

        return this.http.get<Record<string, any>>(url).pipe(
            map(body => {
                const result: { [name: string]: RuleElementDto } = {};
                for (const [key, value] of Object.entries(body)) {
                    result[key] = RuleElementDto.fromJSON(value);
                }

                return result;
            }),
            pretifyError('i18n:rules.loadFailed'));
    }

    public getRules(appName: string): Observable<DynamicRulesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return DynamicRulesDto.fromJSON(body);
            }),
            pretifyError('i18n:rules.loadFailed'));
    }

    public postRule(appName: string, dto: DynamicCreateRuleDto): Observable<DynamicRuleDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.postVersioned(this.http, url, dto.toJSON()).pipe(
            map(({ payload }) => {
                return DynamicRuleDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:rules.createFailed'));
    }

    public putRule(appName: string, resource: Resource, dto: DynamicUpdateRuleDto, version: VersionOrTag): Observable<DynamicRuleDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return DynamicRuleDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:rules.updateFailed'));
    }

    public deleteRule(appName: string, resource: Resource, version: VersionOrTag): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            pretifyError('i18n:rules.deleteFailed'));
    }

    public runRule(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['run'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, {}).pipe(
            pretifyError('i18n:rules.runFailed'));
    }

    public runRuleFromSnapshots(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['run/snapshots'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, {}).pipe(
            pretifyError('i18n:rules.runFailed'));
    }

    public runCancel(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/run`);

        return this.http.delete(url).pipe(
            pretifyError('i18n:rules.cancelFailed'));
    }

    public triggerRule(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['trigger'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, {}).pipe(
            pretifyError('i18n:rules.triggerFailed'));
    }

    public getEvents(appName: string, take: number, skip: number, ruleId?: string): Observable<RuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events${StringHelper.buildQuery({ take, skip, ruleId })}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return RuleEventsDto.fromJSON(body);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public getSimulatedEvents(appName: string, ruleId: string): Observable<SimulatedRuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${ruleId}/simulate`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return SimulatedRuleEventsDto.fromJSON(body);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public postSimulatedEvents(appName: string, trigger: any, flow: any): Observable<SimulatedRuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/simulate`);

        return this.http.post<any>(url, { trigger, flow }).pipe(
            map(body => {
                return SimulatedRuleEventsDto.fromJSON(body);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public enqueueEvent(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url).pipe(
            pretifyError('i18n:rules.ruleEvents.enqueueFailed'));
    }

    public cancelEvents(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['cancel'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url).pipe(
            pretifyError('i18n:rules.ruleEvents.cancelFailed'));
    }

    public getCompletions(appName: string, actionType: string): Observable<ScriptCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/completion/${actionType}`);

        return this.http.get<ScriptCompletions>(url);
    }
}
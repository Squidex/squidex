/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    mapVersioned,
    Model,
    pretifyError,
    ResultSet,
    Version,
    Versioned
} from '@app/framework';

export const ALL_TRIGGERS = {
    'ContentChanged': {
        description: 'For content changes like created, updated, published, unpublished...',
        display: 'Content changed',
        iconColor: '#3389ff',
        iconCode: 'contents'
    },
    'AssetChanged': {
        description: 'For asset changes like uploaded, updated (reuploaded), renamed, deleted...',
        display: 'Asset changed',
        iconColor: '#3389ff',
        iconCode: 'assets'
    },
    'SchemaChanged': {
        description: 'When a schema definition has been created, updated, published or deleted...',
        display: 'Schema changed',
        iconColor: '#3389ff',
        iconCode: 'schemas'},
    'Usage': {
        description: 'When monthly API calls exceed a specified limit for one time a month...',
        display: 'Usage exceeded',
        iconColor: '#3389ff',
        iconCode: 'dashboard'
    }
};

export class RuleElementDto {
    constructor(
        public readonly display: string,
        public readonly description: string,
        public readonly iconColor: string,
        public readonly iconImage: string,
        public readonly iconCode: string | null,
        public readonly readMore: string,
        public readonly properties: RuleElementPropertyDto[]
    ) {
    }
}

export class RuleElementPropertyDto {
    constructor(
        public readonly name: string,
        public readonly editor: string,
        public readonly display: string,
        public readonly description: string,
        public readonly isFormattable: boolean,
        public readonly isRequired: boolean
    ) {
    }
}

export class RuleDto extends Model<RuleDto> {
    constructor(
        public readonly id: string,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly version: Version,
        public readonly isEnabled: boolean,
        public readonly trigger: any,
        public readonly triggerType: string,
        public readonly action: any,
        public readonly actionType: string
    ) {
        super();
    }
}

export class RuleEventsDto extends ResultSet<RuleEventDto> { }

export class RuleEventDto extends Model<RuleEventDto> {
    constructor(
        public readonly id: string,
        public readonly created: DateTime,
        public readonly nextAttempt: DateTime | null,
        public readonly eventName: string,
        public readonly description: string,
        public readonly lastDump: string,
        public readonly result: string,
        public readonly jobResult: string,
        public readonly numCalls: number
    ) {
        super();
    }
}

export interface UpsertRuleDto {
    readonly trigger: RuleAction;
    readonly action: RuleAction;
}

export interface RuleCreatedDto {
    readonly id: string;
}

export type RuleAction = { actionType: string } & any;
export type RuleTrigger = { triggerType: string } & any;

@Injectable()
export class RulesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getActions(): Observable<{ [name: string]: RuleElementDto }> {
        const url = this.apiUrl.buildUrl('api/rules/actions');

        return HTTP.getVersioned<any>(this.http, url).pipe(
            map(({ payload }) => {
                const items: { [name: string]: any } = payload.body;

                const actions: { [name: string]: RuleElementDto } = {};

                for (let key of Object.keys(items).sort()) {
                    const value = items[key];

                    const properties = value.properties.map((property: any) =>
                        new RuleElementPropertyDto(
                            property.name,
                            property.editor,
                            property.display,
                            property.description,
                            property.isFormattable,
                            property.isRequired
                        ));

                    actions[key] = new RuleElementDto(
                        value.display,
                        value.description,
                        value.iconColor,
                        value.iconImage, null,
                        value.readMore,
                        properties);
                }

                return actions;
            }),
            pretifyError('Failed to load Rules. Please reload.'));
    }

    public getRules(appName: string): Observable<RuleDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
            map(({ payload }) => {
                const items: any[] = payload.body;

                const rules = items.map(item =>
                    new RuleDto(
                        item.id,
                        item.createdBy,
                        item.lastModifiedBy,
                        DateTime.parseISO_UTC(item.created),
                        DateTime.parseISO_UTC(item.lastModified),
                        new Version(item.version.toString()),
                        item.isEnabled,
                        item.trigger,
                        item.trigger.triggerType,
                        item.action,
                        item.action.actionType));

                return rules;
            }),
            pretifyError('Failed to load Rules. Please reload.'));
    }

    public postRule(appName: string, dto: UpsertRuleDto): Observable<Versioned<RuleCreatedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.postVersioned<RuleCreatedDto>(this.http, url, dto).pipe(
            mapVersioned(({ body }) => body!),
            tap(() => {
                this.analytics.trackEvent('Rule', 'Created', appName);
            }),
            pretifyError('Failed to create rule. Please reload.'));
    }

    public putRule(appName: string, id: string, dto: Partial<UpsertRuleDto>, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            }),
            pretifyError('Failed to update rule. Please reload.'));
    }

    public enableRule(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}/enable`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            }),
            pretifyError('Failed to enable rule. Please reload.'));
    }

    public disableRule(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}/disable`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            }),
            pretifyError('Failed to disable rule. Please reload.'));
    }

    public deleteRule(appName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Deleted', appName);
            }),
            pretifyError('Failed to delete rule. Please reload.'));
    }

    public getEvents(appName: string, take: number, skip: number): Observable<RuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events?take=${take}&skip=${skip}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
            map(({ payload }) => {
                const body = payload.body;

                const items: any[] = body.items;

                const ruleEvents = new RuleEventsDto(body.total, items.map(item =>
                    new RuleEventDto(
                        item.id,
                        DateTime.parseISO_UTC(item.created),
                        item.nextAttempt ? DateTime.parseISO_UTC(item.nextAttempt) : null,
                        item.eventName,
                        item.description,
                        item.lastDump,
                        item.result,
                        item.jobResult,
                        item.numCalls)));

                return ruleEvents;
            }),
            pretifyError('Failed to load events. Please reload.'));
    }

    public enqueueEvent(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events/${id}`);

        return HTTP.putVersioned(this.http, url, {}).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'EventEnqueued', appName);
            }),
            pretifyError('Failed to enqueue rule event. Please reload.'));
    }

    public cancelEvent(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events/${id}`);

        return HTTP.deleteVersioned(this.http, url).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'EventDequeued', appName);
            }),
            pretifyError('Failed to cancel rule event. Please reload.'));
    }
}
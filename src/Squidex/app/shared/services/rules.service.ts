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
    hasAnyLink,
    HTTP,
    Model,
    pretifyError,
    Resource,
    ResourceLinks,
    ResultSet,
    Version
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
        iconCode: 'schemas'
    },
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

export class RulesDto extends ResultSet<RuleDto> {
    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }

    public get canReadEvents() {
        return hasAnyLink(this._links, 'events');
    }

    constructor(items: RuleDto[], links?: {}) {
        super(items.length, items, links);
    }
}

export class RuleDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canDisable: boolean;
    public readonly canEnable: boolean;
    public readonly canUpdate: boolean;

    constructor(
        links: ResourceLinks,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version,
        public readonly isEnabled: boolean,
        public readonly trigger: any,
        public readonly triggerType: string,
        public readonly action: any,
        public readonly actionType: string
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDisable = hasAnyLink(links, 'disable');
        this.canEnable = hasAnyLink(links, 'enable');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export class RuleEventsDto extends ResultSet<RuleEventDto> {
    public readonly _links: ResourceLinks;
}

export class RuleEventDto extends Model<RuleEventDto> {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
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

        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export interface UpsertRuleDto {
    readonly trigger: RuleAction;
    readonly action: RuleAction;
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

        return HTTP.getVersioned(this.http, url).pipe(
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

    public getRules(appName: string): Observable<RulesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return this.http.get<{ items: [] } & Resource>(url).pipe(
            map(({ items, _links }) => {
                const rules = items.map(item => parseRule(item));

                return new RulesDto(rules, _links);
            }),
            pretifyError('Failed to load Rules. Please reload.'));
    }

    public postRule(appName: string, dto: UpsertRuleDto): Observable<RuleDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
            map(({ payload }) => {
                return parseRule(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Rule', 'Created', appName);
            }),
            pretifyError('Failed to create rule. Please reload.'));
    }

    public putRule(appName: string, resource: Resource, dto: Partial<UpsertRuleDto>, version: Version): Observable<RuleDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseRule(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            }),
            pretifyError('Failed to update rule. Please reload.'));
    }

    public enableRule(appName: string, resource: Resource, version: Version): Observable<RuleDto> {
        const link = resource._links['enable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseRule(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Rule', 'Enabled', appName);
            }),
            pretifyError('Failed to enable rule. Please reload.'));
    }

    public disableRule(appName: string, resource: Resource, version: Version): Observable<RuleDto> {
        const link = resource._links['disable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseRule(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Rule', 'Disabled', appName);
            }),
            pretifyError('Failed to disable rule. Please reload.'));
    }

    public deleteRule(appName: string, resource: Resource, version: Version): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Deleted', appName);
            }),
            pretifyError('Failed to delete rule. Please reload.'));
    }

    public getEvents(appName: string, take: number, skip: number): Observable<RuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events?take=${take}&skip=${skip}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                const body = payload.body;

                const items: any[] = body.items;

                const ruleEvents = new RuleEventsDto(body.total, items.map(item =>
                    new RuleEventDto(item._links,
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

    public enqueueEvent(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'EventEnqueued', appName);
            }),
            pretifyError('Failed to enqueue rule event. Please reload.'));
    }

    public cancelEvent(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'EventDequeued', appName);
            }),
            pretifyError('Failed to cancel rule event. Please reload.'));
    }
}

function parseRule(response: any) {
    return new RuleDto(response._links,
        response.id,
        DateTime.parseISO_UTC(response.created), response.createdBy,
        DateTime.parseISO_UTC(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
        response.isEnabled,
        response.trigger,
        response.trigger.triggerType,
        response.action,
        response.action.actionType);
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, hasAnyLink, HTTP, Model, pretifyError, Resource, ResourceLinks, ResultSet, Version } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

export type RuleElementMetadataDto = Readonly<{
    description: string;
    display: string;
    iconColor: string;
    iconCode: string;
    title?: string;
}>;

export type TriggerType =
    'AssetChanged' |
    'Comment' |
    'ContentChanged' |
    'Manual' |
    'SchemaChanged' |
    'Usage';

export type TriggersDto = Record<TriggerType, RuleElementMetadataDto>;

export const ALL_TRIGGERS: TriggersDto = {
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

export class RuleElementDto {
    constructor(
        public readonly title: string,
        public readonly display: string,
        public readonly description: string,
        public readonly iconColor: string,
        public readonly iconImage: string,
        public readonly iconCode: string | null,
        public readonly readMore: string,
        public readonly properties: ReadonlyArray<RuleElementPropertyDto>,
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
        public readonly isRequired: boolean,
        public readonly options?: ReadonlyArray<string>,
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

    public get canCancelRun() {
        return hasAnyLink(this._links, 'run/cancel');
    }

    constructor(items: ReadonlyArray<RuleDto>, links?: {},
        public readonly runningRuleId?: string,
    ) {
        super(items.length, items, links);
    }
}

export class RuleDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canDisable: boolean;
    public readonly canEnable: boolean;
    public readonly canRun: boolean;
    public readonly canRunFromSnapshots: boolean;
    public readonly canTrigger: boolean;
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
        public readonly actionType: string,
        public readonly name: string,
        public readonly numSucceeded: number,
        public readonly numFailed: number,
        public readonly lastExecuted?: DateTime,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDisable = hasAnyLink(links, 'disable');
        this.canEnable = hasAnyLink(links, 'enable');
        this.canRun = hasAnyLink(links, 'run');
        this.canRunFromSnapshots = hasAnyLink(links, 'run/snapshots');
        this.canTrigger = hasAnyLink(links, 'logs');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export class RuleEventsDto extends ResultSet<RuleEventDto> {
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
        public readonly numCalls: number,
    ) {
        super();

        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export class SimulatedRuleEventsDto extends ResultSet<SimulatedRuleEventDto> {
}

export class SimulatedRuleEventDto {
    public readonly _links: ResourceLinks;

    constructor(links: ResourceLinks,
        public readonly eventName: string,
        public readonly event: any,
        public readonly enrichedEvent: any | undefined,
        public readonly actionName: string | undefined,
        public readonly actionData: string | undefined,
        public readonly error: string | undefined,
        public readonly skipReasons: ReadonlyArray<string>,
    ) {
        this._links = links;
    }
}

export type ActionsDto =
    Readonly<{ [name: string]: RuleElementDto }>;

export type UpsertRuleDto =
    Readonly<{ trigger?: RuleTrigger; action?: RuleAction; name?: string; isEnabled?: boolean }>;

export type RuleAction =
    Readonly<{ actionType: string; [key: string]: any }>;

export type RuleTrigger =
    Readonly<{ triggerType: string; [key: string]: any }>;

@Injectable()
export class RulesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getActions(): Observable<{ [name: string]: RuleElementDto }> {
        const url = this.apiUrl.buildUrl('api/rules/actions');

        return this.http.get<any>(url).pipe(
            map(body => {
                const actions = parseActions(body);

                return actions;
            }),
            pretifyError('i18n:rules.loadFailed'));
    }

    public getRules(appName: string): Observable<RulesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return this.http.get<{ items: [] } & Resource & { runningRuleId?: string }>(url).pipe(
            map(({ items, _links, runningRuleId }) => {
                const rules = items.map(parseRule);

                return new RulesDto(rules, _links, runningRuleId);
            }),
            pretifyError('i18n:rules.loadFailed'));
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
            pretifyError('i18n:rules.createFailed'));
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
            pretifyError('i18n:rules.updateFailed'));
    }

    public deleteRule(appName: string, resource: Resource, version: Version): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Deleted', appName);
            }),
            pretifyError('i18n:rules.deleteFailed'));
    }

    public runRule(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['run'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, {}).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Run', appName);
            }),
            pretifyError('i18n:rules.runFailed'));
    }

    public runRuleFromSnapshots(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['run/snapshots'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, {}).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Run', appName);
            }),
            pretifyError('i18n:rules.runFailed'));
    }

    public runCancel(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/run`);

        return this.http.delete(url).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'RunCancel', appName);
            }),
            pretifyError('i18n:rules.cancelFailed'));
    }

    public triggerRule(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['trigger'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, {}).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'Triggered', appName);
            }),
            pretifyError('i18n:rules.triggerFailed'));
    }

    public getEvents(appName: string, take: number, skip: number, ruleId?: string): Observable<RuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events?take=${take}&skip=${skip}&ruleId=${ruleId || ''}`);

        return this.http.get<{ items: any[]; total: number } & Resource>(url).pipe(
            map(({ items, total, _links }) => {
                const ruleEvents = items.map(parseRuleEvent);

                return new RuleEventsDto(total, ruleEvents, _links);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public getSimulatedEvents(appName: string, ruleId: string): Observable<SimulatedRuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${ruleId}/simulate`);

        return this.http.get<{ items: any[]; total: number } & Resource>(url).pipe(
            map(({ items, total, _links }) => {
                const simulatedRuleEvents = items.map(parseSimulatedRuleEvent);

                return new SimulatedRuleEventsDto(total, simulatedRuleEvents, _links);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public enqueueEvent(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'EventEnqueued', appName);
            }),
            pretifyError('i18n:rules.ruleEvents.enqueueFailed'));
    }

    public cancelEvents(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['cancel'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('Rule', 'EventsCancelled', appName);
            }),
            pretifyError('i18n:rules.ruleEvents.cancelFailed'));
    }
}

function parseActions(response: any) {
    const actions: { [name: string]: RuleElementDto } = {};

    for (const key of Object.keys(response).sort()) {
        const value = response[key];

        const properties = value.properties.map((property: any) =>
            new RuleElementPropertyDto(
                property.name,
                property.editor,
                property.display,
                property.description,
                property.isFormattable,
                property.isRequired,
                property.options,
            ));

        actions[key] = new RuleElementDto(
            value.title,
            value.display,
            value.description,
            value.iconColor,
            value.iconImage, null,
            value.readMore,
            properties);
    }

    return actions;
}

function parseRule(response: any) {
    return new RuleDto(response._links,
        response.id,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
        response.isEnabled,
        response.trigger,
        response.trigger.triggerType,
        response.action,
        response.action.actionType,
        response.name,
        response.numSucceeded,
        response.numFailed,
        response.lastExecuted ? DateTime.parseISO(response.lastExecuted) : undefined);
}

function parseRuleEvent(response: any) {
    return new RuleEventDto(response._links,
        response.id,
        DateTime.parseISO(response.created),
        response.nextAttempt ? DateTime.parseISO(response.nextAttempt) : null,
        response.eventName,
        response.description,
        response.lastDump,
        response.result,
        response.jobResult,
        response.numCalls);
}

function parseSimulatedRuleEvent(response: any) {
    return new SimulatedRuleEventDto(response._links,
        response.eventName,
        response.event,
        response.enrichedEvent,
        response.actionName,
        response.actionData,
        response.error,
        response.skipReasons);
}

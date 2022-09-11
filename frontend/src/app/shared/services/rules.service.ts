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
import { ApiUrlConfig, DateTime, hasAnyLink, HTTP, Model, pretifyError, Resource, ResourceLinks, Version } from '@app/framework';

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

export class RuleDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canDisable: boolean;
    public readonly canEnable: boolean;
    public readonly canReadLogs: boolean;
    public readonly canRun: boolean;
    public readonly canRunFromSnapshots: boolean;
    public readonly canTrigger: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
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
        this.canReadLogs = hasAnyLink(links, 'logs');
        this.canRun = hasAnyLink(links, 'run');
        this.canRunFromSnapshots = hasAnyLink(links, 'run/snapshots');
        this.canTrigger = hasAnyLink(links, 'trigger');
        this.canUpdate = hasAnyLink(links, 'update');
    }
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

        this.canDelete = hasAnyLink(links, 'cancel');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export class SimulatedRuleEventDto {
    public readonly _links: ResourceLinks;

    constructor(links: ResourceLinks,
        public readonly eventId: string,
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

export type RuleCompletions = ReadonlyArray<Readonly<{
    // The autocompletion path.
    path: string;

    // The description of the autocompletion field.
    description: string;

    // The type of the autocompletion field.
    type: string;
}>>;

export type RulesDto = Readonly<{
    // The list of rules.
    items: ReadonlyArray<RuleDto>;

    // The id of the rule that is currently running.
    runningRuleId?: string;

    // True, if the user has permission to create a rule.
    canCreate?: boolean;

    // True, if the user has permission to read events.
    canReadEvents?: boolean;

    // True, if the user has permission to cancel an event.
    canCancelRun?: boolean;
}>;

export type RuleEventsDto = Readonly<{
    // The list of rule events.
    items: ReadonlyArray<RuleEventDto>;

    // The total number of rule events.
    total: number;

    // True, if the user has permissions to cancel all rule events.
    canCancelAll?: boolean;
}> & Resource;

export type SimulatedRuleEventsDto = Readonly<{
    // The list of simulated rule events.
    items: ReadonlyArray<SimulatedRuleEventDto>;

    // The total number of simulated rule events.
    total: number;
}>;

export type ActionsDto = Readonly<{
    // The rule elements by name.
    [name: string]: RuleElementDto;
}>;

export type UpsertRuleDto = Readonly<{
    // The optional trigger to update.
    trigger?: RuleTrigger;

    // The optional action to update.
    action?: RuleAction;

    // The optional rule name.
    name?: string;

    // True, if the rule is enabled.
    isEnabled?: boolean;
}>;

export type RuleAction = Readonly<{
    // The type of the action.
    actionType: string;

    // The additional properties.
    [key: string]: any;
 }>;

export type RuleTrigger = Readonly<{
    // The type of the trigger.
    triggerType: string;

    // The additional properties.
    [key: string]: any;
}>;

@Injectable()
export class RulesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getActions(): Observable<{ [name: string]: RuleElementDto }> {
        const url = this.apiUrl.buildUrl('api/rules/actions');

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseActions(body);
            }),
            pretifyError('i18n:rules.loadFailed'));
    }

    public getRules(appName: string): Observable<RulesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseRules(body);
            }),
            pretifyError('i18n:rules.loadFailed'));
    }

    public postRule(appName: string, dto: UpsertRuleDto): Observable<RuleDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
            map(({ payload }) => {
                return parseRule(payload.body);
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
            pretifyError('i18n:rules.updateFailed'));
    }

    public deleteRule(appName: string, resource: Resource, version: Version): Observable<any> {
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
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events?take=${take}&skip=${skip}&ruleId=${ruleId || ''}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseEvents(body);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public getSimulatedEvents(appName: string, ruleId: string): Observable<SimulatedRuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${ruleId}/simulate`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseSimulatedEvents(body);
            }),
            pretifyError('i18n:rules.ruleEvents.loadFailed'));
    }

    public postSimulatedEvents(appName: string, trigger: any, action: any): Observable<SimulatedRuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/simulate`);

        return this.http.post<any>(url, { trigger, action }).pipe(
            map(body => {
                return parseSimulatedEvents(body);
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

    public getCompletions(appName: string, actionType: string): Observable<RuleCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/completion/${actionType}`);

        return this.http.get<RuleCompletions>(url);
    }
}

function parseSimulatedEvents(response: { items: any[]; total: number } & Resource): SimulatedRuleEventsDto {
    const { items: list, total } = response;
    const items = list.map(parseSimulatedRuleEvent);

    return { items, total };
}

function parseEvents(response: { items: any[]; total: number } & Resource): RuleEventsDto {
    const { items: list, total, _links } = response;
    const items = list.map(parseRuleEvent);

    const canCancelAll = hasAnyLink(_links, 'create');

    return { items, total, canCancelAll, _links };
}

function parseRules(response: { items: any[]; runningRuleId?: string } & Resource): RulesDto {
    const { items: list, runningRuleId, _links } = response;
    const items = list.map(parseRule);

    const canCreate = hasAnyLink(_links, 'create');
    const canReadEvents = hasAnyLink(_links, 'events');
    const canCancelRun = hasAnyLink(_links, 'run/cancel');

    return { items, runningRuleId, canCreate, canCancelRun, canReadEvents };
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
        response.eventId,
        response.eventName,
        response.event,
        response.enrichedEvent,
        response.actionName,
        response.actionData,
        response.error,
        response.skipReasons);
}

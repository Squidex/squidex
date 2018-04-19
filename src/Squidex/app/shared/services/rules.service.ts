/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import '@app/framework/angular/http/http-extensions';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    Version,
    Versioned
} from '@app/framework';

export const ruleTriggers: any = {
    'AssetChanged': {
        name: 'Asset changed'
    },
    'ContentChanged': {
        name: 'Content changed'
    }
};

export const ruleActions: any = {
    'Algolia': {
        name: 'Populate Algolia Index'
    },
    'AzureQueue': {
        name: 'Send to Azure Queue'
    },
    'ElasticSearch': {
        name: 'Populate ElasticSearch Index'
    },
    'Fastly': {
        name: 'Purge fastly Cache'
    },
    'Slack': {
        name: 'Send to Slack'
    },
    'Webhook': {
        name: 'Send Webhook'
    }
};

export class RuleDto {
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
    }
}

export class RuleEventDto {
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
    }
}

export class RuleEventsDto {
    constructor(
        public readonly total: number,
        public readonly items: RuleEventDto[]
    ) {
    }
}

export class CreateRuleDto {
    constructor(
        public readonly trigger: any,
        public readonly action: any
    ) {
    }
}

export class UpdateRuleDto {
    constructor(
        public readonly trigger: any,
        public readonly action: any
    ) {
    }
}

@Injectable()
export class RulesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getRules(appName: string): Observable<RuleDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const items: any[] = response.payload.body;

                return items.map(item => {
                    return new RuleDto(
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
                        item.action.actionType);
                });
            })
            .pretifyError('Failed to load Rules. Please reload.');
    }

    public postRule(appName: string, dto: CreateRuleDto, user: string, now: DateTime): Observable<RuleDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules`);

        return HTTP.postVersioned<any>(this.http, url, dto)
            .map(response => {
                const body = response.payload.body;

                return new RuleDto(
                    body.id,
                    user,
                    user,
                    now,
                    now,
                    response.version,
                    true,
                    dto.trigger,
                    dto.trigger.triggerType,
                    dto.action,
                    dto.action.actionType);
            })
            .do(() => {
                this.analytics.trackEvent('Rule', 'Created', appName);
            })
            .pretifyError('Failed to create rule. Please reload.');
    }

    public putRule(appName: string, id: string, dto: UpdateRuleDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .do(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            })
            .pretifyError('Failed to update rule. Please reload.');
    }

    public enableRule(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}/enable`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            })
            .pretifyError('Failed to enable rule. Please reload.');
    }

    public disableRule(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}/disable`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Rule', 'Updated', appName);
            })
            .pretifyError('Failed to disable rule. Please reload.');
    }

    public deleteRule(appName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .do(() => {
                this.analytics.trackEvent('Rule', 'Deleted', appName);
            })
            .pretifyError('Failed to delete rule. Please reload.');
    }

    public getEvents(appName: string, take: number, skip: number): Observable<RuleEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events?take=${take}&skip=${skip}`);

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const body = response.payload.body;

                const items: any[] = body.items;

                return new RuleEventsDto(body.total, items.map(item => {
                    return new RuleEventDto(
                        item.id,
                        DateTime.parseISO_UTC(item.created),
                        item.nextAttempt ? DateTime.parseISO_UTC(item.nextAttempt) : null,
                        item.eventName,
                        item.description,
                        item.lastDump,
                        item.result,
                        item.jobResult,
                        item.numCalls);
                }));
            })
            .pretifyError('Failed to load events. Please reload.');
    }

    public enqueueEvent(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/rules/events/${id}`);

        return HTTP.putVersioned(this.http, url, {})
            .do(() => {
                this.analytics.trackEvent('Rule', 'EventEnqueued', appName);
            })
            .pretifyError('Failed to enqueue rule event. Please reload.');
    }
}
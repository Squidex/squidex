/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { DialogService, getPagingInfo, ListState, Resource, shareSubscribed, State } from '@app/framework';
import { RuleEventDto, RulesService } from './../services/rules.service';
import { AppsState } from './apps.state';

interface Snapshot extends ListState {
    // The current rule events.
    ruleEvents: ReadonlyArray<RuleEventDto>;

    // The current rule id.
    ruleId?: string;

    // True, if the user has permissions to cancel all rule events.
    canCancelAll?: boolean;

    // The resource.
    resource: Resource;
}

@Injectable()
export class RuleEventsState extends State<Snapshot> {
    public ruleEvents =
        this.project(x => x.ruleEvents);

    public paging =
        this.project(x => getPagingInfo(x, x.ruleEvents.length));

    public query =
        this.project(x => x.query);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCancelAll =
        this.project(x => x.canCancelAll === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService,
    ) {
        super({
            resource: { _links: {} },
            ruleEvents: [],
            page: 0,
            pageSize: 30,
            total: 0,
        }, 'Rule Events');
    }

    public load(isReload = false, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            const { ruleId } = this.snapshot;

            this.resetState({ ruleId, ...update }, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        const { page, pageSize, ruleId } = this.snapshot;

        return this.rulesService.getEvents(this.appName,
                pageSize,
                pageSize * page,
                ruleId).pipe(
            tap(payload => {
                const { total, items: ruleEvents, canCancelAll } = payload;

                if (isReload) {
                    this.dialogs.notifyInfo('i18n:rules.ruleEvents.reloaded');
                }

                return this.next({
                    canCancelAll,
                    isLoaded: true,
                    isLoading: false,
                    resource: payload,
                    ruleEvents,
                    total,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public enqueue(event: RuleEventDto): Observable<any> {
        return this.rulesService.enqueueEvent(this.appsState.appName, event).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.ruleEvents.enqueued');
            }),
            shareSubscribed(this.dialogs));
    }

    public cancelAll(): Observable<any> {
        return this.rulesService.cancelEvents(this.appsState.appName, this.snapshot.resource).pipe(
            tap(() => {
                return this.next(s => {
                    const ruleEvents = s.ruleEvents.map(x => setCancelled(x));

                    return { ...s, ruleEvents, isLoaded: true };
                }, 'CancelAll');
            }),
            shareSubscribed(this.dialogs));
    }

    public cancel(event: RuleEventDto): Observable<any> {
        return this.rulesService.cancelEvents(this.appsState.appName, event).pipe(
            tap(() => {
                return this.next(s => {
                    const ruleEvents = s.ruleEvents.replacedBy('id', setCancelled(event));

                    return { ...s, ruleEvents, isLoaded: true };
                }, 'Cancel');
            }),
            shareSubscribed(this.dialogs));
    }

    public filterByRule(ruleId?: string) {
        if (!this.next({ page: 0, ruleId }, 'Loading Rule')) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    public page(paging: { page: number; pageSize: number }) {
        if (!this.next(paging, 'Loading Paged')) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }
}

const setCancelled = (event: RuleEventDto) =>
    new RuleEventDto(
        event._links,
        event.id,
        event.created,
        null,
        event.eventName,
        event.description,
        event.lastDump,
        event.result,
        'Cancelled',
        event.numCalls);
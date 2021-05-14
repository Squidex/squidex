/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, getPagingInfo, ListState, shareSubscribed, State } from '@app/framework';
import { EMPTY, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { RuleEventDto, RulesService } from './../services/rules.service';
import { AppsState } from './apps.state';

interface Snapshot extends ListState {
    // The current rule events.
    ruleEvents: ReadonlyArray<RuleEventDto>;

    // The current rule id.
    ruleId?: string;
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

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService,
    ) {
        super({
            ruleEvents: [],
            page: 0,
            pageSize: 30,
            total: 0,
        }, 'Rule Events');
    }

    public load(isReload = false, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            this.resetState({ ruleId: this.snapshot.ruleId, ...update }, 'Loading Initial');
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
            tap(({ total, items: ruleEvents }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:rules.ruleEvents.reloaded');
                }

                return this.next({
                    isLoaded: true,
                    isLoading: false,
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

    public cancel(event: RuleEventDto): Observable<any> {
        return this.rulesService.cancelEvent(this.appsState.appName, event).pipe(
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

    private get appName() {
        return this.appsState.appName;
    }
}

const setCancelled = (event: RuleEventDto) =>
    event.with({ nextAttempt: null, jobResult: 'Cancelled' });

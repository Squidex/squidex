/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { empty, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';

import {
    DialogService,
    LocalStoreService,
    Pager,
    shareSubscribed,
    State
} from '@app/framework';

import { AppsState } from './apps.state';

import { RuleEventDto, RulesService } from './../services/rules.service';

interface Snapshot {
    // The current rule events.
    ruleEvents: ReadonlyArray<RuleEventDto>;

    // The pagination information.
    ruleEventsPager: Pager;

    // Indicates if the rule events are loaded.
    isLoaded?: boolean;

    // Indicates if the rule events are loading.
    isLoading?: boolean;

    // The current rule id.
    ruleId?: string;
}

@Injectable()
export class RuleEventsState extends State<Snapshot> {
    public ruleEvents =
        this.project(x => x.ruleEvents);

    public ruleEventsPager =
        this.project(x => x.ruleEventsPager);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly localStore: LocalStoreService,
        private readonly rulesService: RulesService
    ) {
        super({
            ruleEvents: [],
            ruleEventsPager: Pager.fromLocalStore('rule-events', localStore)
        });

        this.ruleEventsPager.subscribe(pager => {
            pager.saveTo('rule-events', this.localStore);
        });
    }

    public load(isReload = false): Observable<any> {
        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false): Observable<any> {
        this.next({ isLoading: true });

        return this.rulesService.getEvents(this.appName,
                this.snapshot.ruleEventsPager.pageSize,
                this.snapshot.ruleEventsPager.skip,
                this.snapshot.ruleId).pipe(
            tap(({ total, items: ruleEvents }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('RuleEvents reloaded.');
                }

                return this.next(s => {
                    const ruleEventsPager = s.ruleEventsPager.setCount(total);

                    return {
                        ...s,
                        isLoaded: true,
                        isLoading: false,
                        ruleEvents,
                        ruleEventsPager
                    };
                });
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }),
            shareSubscribed(this.dialogs));
    }

    public enqueue(event: RuleEventDto): Observable<any> {
        return this.rulesService.enqueueEvent(this.appsState.appName, event).pipe(
            tap(() => {
                this.dialogs.notifyInfo('Events enqueued. Will be resend in a few seconds.');
            }),
            shareSubscribed(this.dialogs));
    }

    public cancel(event: RuleEventDto): Observable<any> {
        return this.rulesService.cancelEvent(this.appsState.appName, event).pipe(
            tap(() => {
                return this.next(s => {
                    const ruleEvents = s.ruleEvents.replaceBy('id', setCancelled(event));

                    return { ...s, ruleEvents, isLoaded: true };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public filterByRule(ruleId?: string) {
        if (ruleId === this.snapshot.ruleId) {
            return empty();
        }

        this.next(s => ({ ...s, ruleEventsPager: s.ruleEventsPager.reset(), ruleId }));

        return this.loadInternal();
    }

    public setPager(ruleEventsPager: Pager): Observable<any> {
        this.next(s => ({ ...s, ruleEventsPager }));

        return this.loadInternal();
    }

    private get appName() {
        return this.appsState.appName;
    }
}

const setCancelled = (event: RuleEventDto) =>
    event.with({ nextAttempt: null, jobResult: 'Cancelled' });
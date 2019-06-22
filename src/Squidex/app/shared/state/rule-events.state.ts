/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    Pager,
    shareSubscribed,
    State
} from '@app/framework';

import { AppsState } from './apps.state';

import { RuleEventDto, RulesService } from './../services/rules.service';

interface Snapshot {
    // The current rule events.
    ruleEvents: ImmutableArray<RuleEventDto>;

    // The pagination information.
    ruleEventsPager: Pager;

    // Indicates if the rule events are loaded.
    isLoaded?: boolean;
}

@Injectable()
export class RuleEventsState extends State<Snapshot> {
    public ruleEvents =
        this.project(x => x.ruleEvents);

    public ruleEventsPager =
        this.project(x => x.ruleEventsPager);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService
    ) {
        super({ ruleEvents: ImmutableArray.of(), ruleEventsPager: new Pager(0) });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false): Observable<any> {
        return this.rulesService.getEvents(this.appName,
                this.snapshot.ruleEventsPager.pageSize,
                this.snapshot.ruleEventsPager.skip).pipe(
            tap(({ total, items }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('RuleEvents reloaded.');
                }

                return this.next(s => {
                    const ruleEvents = ImmutableArray.of(items);
                    const ruleEventsPager = s.ruleEventsPager.setCount(total);

                    return { ...s, ruleEvents, ruleEventsPager, isLoaded: true };
                });
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

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, ruleEventsPager: s.ruleEventsPager.goNext() }));

        return this.loadInternal();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, ruleEventsPager: s.ruleEventsPager.goPrev() }));

        return this.loadInternal();
    }

    private get appName() {
        return this.appsState.appName;
    }
}

const setCancelled = (event: RuleEventDto) =>
    event.with({ nextAttempt: null, jobResult: 'Cancelled' });
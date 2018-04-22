/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DialogService,
    ImmutableArray,
    Pager,
    State
} from '@app/framework';

import { AppsState } from './apps.state';

import { RuleEventDto, RulesService } from './../services/rules.service';

interface Snapshot {
    ruleEvents: ImmutableArray<RuleEventDto>;
    ruleEventsPager: Pager;

    isLoaded?: boolean;
}

@Injectable()
export class RuleEventsState extends State<Snapshot> {
    public ruleEvents =
        this.changes.map(x => x.ruleEvents)
            .distinctUntilChanged();

    public ruleEventsPager =
        this.changes.map(x => x.ruleEventsPager)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

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
                this.snapshot.ruleEventsPager.skip)
            .do(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('RuleEvents reloaded.');
                }

                return this.next(s => {
                    const ruleEvents = ImmutableArray.of(dtos.items);
                    const ruleEventsPager = s.ruleEventsPager.setCount(dtos.total);

                    return { ...s, ruleEvents, ruleEventsPager, isLoaded: true };
                });
            })
            .notify(this.dialogs);
    }

    public enqueue(event: RuleEventDto): Observable<any> {
        return this.rulesService.enqueueEvent(this.appsState.appName, event.id)
            .do(() => {
                this.dialogs.notifyInfo('Events enqueued. Will be resend in a few seconds.');
            })
            .notify(this.dialogs);
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
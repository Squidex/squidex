/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, ListState, shareSubscribed, State } from '@app/framework';
import { EMPTY, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { RulesService, SimulatedRuleEventDto } from './../services/rules.service';
import { AppsState } from './apps.state';

interface Snapshot extends ListState {
    // The current simulated rule events.
    simulatedRuleEvents: ReadonlyArray<SimulatedRuleEventDto>;

    // The current rule id.
    ruleId?: string;
}

@Injectable()
export class RuleSimulatorState extends State<Snapshot> {
    public simulatedRuleEvents =
        this.project(x => x.simulatedRuleEvents);

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
            simulatedRuleEvents: [],
            page: 0,
            pageSize: 0,
            total: 0,
        }, 'Simulated Rule Events');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState({ ruleId: this.snapshot.ruleId }, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        if (!this.snapshot.ruleId) {
            return EMPTY;
        }

        this.next({ isLoading: true }, 'Loading Started');

        const { ruleId } = this.snapshot;

        return this.rulesService.getSimulatedEvents(this.appName, ruleId!).pipe(
            tap(({ total, items: simulatedRuleEvents }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:rules.ruleEvents.reloaded');
                }

                return this.next({
                    isLoaded: true,
                    isLoading: false,
                    simulatedRuleEvents,
                    total,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public selectRule(ruleId?: string) {
        this.resetState({ ruleId }, 'Select Rule');
    }

    private get appName() {
        return this.appsState.appName;
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, ListState, shareSubscribed, State } from '@app/framework';
import { DynamicCreateRuleDto, SimulatedRuleEventDto } from '../model';
import { RulesService } from '../services/rules.service';
import { AppsState } from './apps.state';

interface Snapshot extends ListState {
    // The current simulated rule events.
    simulatedRuleEvents: ReadonlyArray<SimulatedRuleEventDto>;

    // The current rule id.
    ruleId?: string;

    // The rule trigger.
    trigger?: any;

    // The rule flow.
    flow?: any;
}

@Injectable({
    providedIn: 'root',
})
export class RuleSimulatorState extends State<Snapshot> {
    public simulatedRuleEvents =
        this.project(x => x.simulatedRuleEvents);

    public query =
        this.project(x => x.query);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

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
            simulatedRuleEvents: [],
            page: 0,
            pageSize: 0,
            total: 0,
        });

        debug(this, 'ruleSimulator');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            const { flow, ruleId, trigger } = this.snapshot;

            this.resetState({ flow, ruleId, trigger }, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        const { flow, ruleId, trigger } = this.snapshot;

        if (!ruleId && !trigger && !flow) {
            return EMPTY;
        }

        this.next({ isLoading: true }, 'Loading Started');

        const request =
            flow && trigger ?
            this.rulesService.postSimulatedEvents(this.appName, new DynamicCreateRuleDto({ flow, trigger })) :
            this.rulesService.getSimulatedEvents(this.appName, ruleId!);

        return request.pipe(
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

    public setRule(trigger: any, flow: any) {
        this.next({ trigger, flow }, 'Set Rule');
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { finalize, map, tap } from 'rxjs/operators';
import { DialogService, LoadingState, shareSubscribed, State } from '@app/framework';
import { RuleDto, RulesService, UpsertRuleDto } from './../services/rules.service';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current rules.
    rules: ReadonlyArray<RuleDto>;

    // The selected rule.
    selectedRule?: RuleDto | null;

    // The id of the rule that is currently running.
    runningRuleId?: string;

    // Indicates if a rule run can be cancelled.
    canCancelRun?: boolean;

    // Indicates if the user can create rules.
    canCreate?: boolean;

    // Indicates if the user can read events.
    canReadEvents?: boolean;
}

@Injectable()
export class RulesState extends State<Snapshot> {
    public selectedRule =
        this.project(x => x.selectedRule);

    public rules =
        this.project(x => x.rules);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public canCancelRun =
        this.project(x => x.canCancelRun === true);

    public canReadEvents =
        this.project(x => x.canReadEvents === true);

    public runningRuleId =
        this.project(x => x.runningRuleId);

    public runningRule =
        this.projectFrom2(this.rules, this.runningRuleId, (r, id) => r.find(x => x.id === id));

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
        super({ rules: [] }, 'Rules');
    }

    public select(id: string | null): Observable<RuleDto | null> {
        return this.loadIfNotLoaded().pipe(
            map(() => this.snapshot.rules.find(x => x.id === id) || null),
            tap(selectedRule => {
                this.next({ selectedRule });
            }));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            const { selectedRule } = this.snapshot;

            this.resetState({ selectedRule }, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return of({});
        }

        return this.loadInternal(false);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.rulesService.getRules(this.appName).pipe(
            tap(({ items: rules, runningRuleId, canCancelRun, canCreate, canReadEvents }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:rules.reloaded');
                }

                this.next(s => {
                    let selectedRule = s.selectedRule;

                    if (selectedRule) {
                        selectedRule = rules.find(x => x.id === selectedRule!.id);
                    }

                    return {
                        canCancelRun,
                        canCreate,
                        canReadEvents,
                        isLoaded: true,
                        isLoading: false,
                        runningRuleId,
                        rules,
                        selectedRule,
                    };
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: UpsertRuleDto): Observable<RuleDto> {
        return this.rulesService.postRule(this.appName, request).pipe(
            tap(created => {
                this.next(s => {
                    const rules = [...s.rules, created];

                    return { ...s, rules };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(rule: RuleDto): Observable<any> {
        return this.rulesService.deleteRule(this.appName, rule, rule.version).pipe(
            tap(() => {
                this.next(s => {
                    const rules = s.rules.removedBy('id', rule);

                    const selectedRule =
                        s.selectedRule?.id !== rule.id ?
                        s.selectedRule :
                        null;

                    return { ...s, rules, selectedRule };
                }, 'Deleted');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(rule: RuleDto, dto: Partial<UpsertRuleDto>): Observable<RuleDto> {
        return this.rulesService.putRule(this.appName, rule, dto, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public run(rule: RuleDto): Observable<any> {
        return this.rulesService.runRule(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.restarted');
            }),
            shareSubscribed(this.dialogs));
    }

    public runFromSnapshots(rule: RuleDto): Observable<any> {
        return this.rulesService.runRuleFromSnapshots(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.restarted');
            }),
            shareSubscribed(this.dialogs));
    }

    public trigger(rule: RuleDto): Observable<any> {
        return this.rulesService.triggerRule(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.enqueued');
            }),
            shareSubscribed(this.dialogs));
    }

    public runCancel(): Observable<any> {
        return this.rulesService.runCancel(this.appName).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.stop');
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceRule(rule: RuleDto) {
        this.next(s => {
            const rules = s.rules.replacedBy('id', rule);

            const selectedRule =
                s.selectedRule?.id !== rule.id ?
                s.selectedRule :
                rule;

            return { ...s, rules, selectedRule };
        }, 'Updated');
    }
}

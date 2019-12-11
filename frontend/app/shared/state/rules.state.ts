/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';

import {
    DialogService,
    shareSubscribed,
    State
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    RuleDto,
    RulesService,
    UpsertRuleDto
} from './../services/rules.service';

interface Snapshot {
    // The current rules.
    rules: RulesList;

    // Indicates if the rules are loaded.
    isLoaded?: boolean;

    // Indicates if the rules are loading.
    isLoading?: boolean;

    // Indicates if the user can create rules.
    canCreate?: boolean;

    // Indicates if the user can read events.
    canReadEvents?: boolean;
}

type RulesList = ReadonlyArray<RuleDto>;

@Injectable()
export class RulesState extends State<Snapshot> {
    public rules =
        this.project(x => x.rules);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public canReadEvents =
        this.project(x => x.canReadEvents === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService
    ) {
        super({ rules: [] });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true });

        return this.rulesService.getRules(this.appName).pipe(
            tap(({ items: rules, canCreate, canReadEvents }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Rules reloaded.');
                }

                this.next({
                    canCreate,
                    canReadEvents,
                    isLoaded: true,
                    isLoading: false,
                    rules
                });
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: UpsertRuleDto): Observable<RuleDto> {
        return this.rulesService.postRule(this.appName, request).pipe(
            tap(created => {
                this.next(s => {
                    const rules = [...s.rules, created];

                    return { ...s, rules };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(rule: RuleDto): Observable<any> {
        return this.rulesService.deleteRule(this.appName, rule, rule.version).pipe(
            tap(() => {
                this.next(s => {
                    const rules = s.rules.removeBy('id', rule);

                    return { ...s, rules };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public updateAction(rule: RuleDto, action: any): Observable<RuleDto> {
        return this.rulesService.putRule(this.appName, rule, { action }, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public updateTrigger(rule: RuleDto, trigger: any): Observable<RuleDto> {
        return this.rulesService.putRule(this.appName, rule, { trigger }, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public rename(rule: RuleDto, name: string): Observable<RuleDto> {
        return this.rulesService.putRule(this.appName, rule, { name }, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public enable(rule: RuleDto): Observable<any> {
        return this.rulesService.enableRule(this.appName, rule, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public disable(rule: RuleDto): Observable<any> {
        return this.rulesService.disableRule(this.appName, rule, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public trigger(rule: RuleDto): Observable<any> {
        return this.rulesService.triggerRule(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('Rule has been added to the queue.');
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceRule(rule: RuleDto) {
        this.next(s => {
            const rules = s.rules.replaceBy('id', rule);

            return { ...s, rules };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }
}
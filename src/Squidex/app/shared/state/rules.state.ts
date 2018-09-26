/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    notify,
    State,
    Version
} from '@app/framework';

import { AuthService} from './../services/auth.service';
import { AppsState } from './apps.state';

import {
    CreateRuleDto,
    RuleDto,
    RulesService,
    UpdateRuleDto
} from './../services/rules.service';

interface Snapshot {
    rules: ImmutableArray<RuleDto>;

    isLoaded?: boolean;
}

@Injectable()
export class RulesState extends State<Snapshot> {
    public rules =
        this.changes.pipe(map(x => x.rules),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService
    ) {
        super({ rules: ImmutableArray.empty() });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.rulesService.getRules(this.appName).pipe(
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Rules reloaded.');
                }

                this.next(s => {
                    const rules = ImmutableArray.of(dtos);

                    return { ...s, rules, isLoaded: true };
                });
            }),
            notify(this.dialogs));
    }

    public create(request: CreateRuleDto, now?: DateTime): Observable<any> {
        return this.rulesService.postRule(this.appName, request, this.user, now || DateTime.now()).pipe(
            tap(dto => {
                this.next(s => {
                    const rules = s.rules.push(dto);

                    return { ...s, rules };
                });
            }),
            notify(this.dialogs));
    }

    public delete(rule: RuleDto): Observable<any> {
        return this.rulesService.deleteRule(this.appName, rule.id, rule.version).pipe(
            tap(dto => {
                this.next(s => {
                    const rules = s.rules.removeAll(x => x.id === rule.id);

                    return { ...s, rules };
                });
            }),
            notify(this.dialogs));
    }

    public updateAction(rule: RuleDto, action: any, now?: DateTime): Observable<any> {
        return this.rulesService.putRule(this.appName, rule.id, new UpdateRuleDto(null, action), rule.version).pipe(
            tap(dto => {
                this.replaceRule(updateAction(rule, action, this.user, dto.version, now));
            }),
            notify(this.dialogs));
    }

    public updateTrigger(rule: RuleDto, trigger: any, now?: DateTime): Observable<any> {
        return this.rulesService.putRule(this.appName, rule.id, new UpdateRuleDto(trigger, null), rule.version).pipe(
            tap(dto => {
                this.replaceRule(updateTrigger(rule, trigger, this.user, dto.version, now));
            }),
            notify(this.dialogs));
    }

    public enable(rule: RuleDto, now?: DateTime): Observable<any> {
        return this.rulesService.enableRule(this.appName, rule.id, rule.version).pipe(
            tap(dto => {
                this.replaceRule(setEnabled(rule, true, this.user, dto.version, now));
            }),
            notify(this.dialogs));
    }

    public disable(rule: RuleDto, now?: DateTime): Observable<any> {
        return this.rulesService.disableRule(this.appName, rule.id, rule.version).pipe(
            tap(dto => {
                this.replaceRule(setEnabled(rule, false, this.user, dto.version, now));
            }),
            notify(this.dialogs));
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

    private get user() {
        return this.authState.user!.token;
    }
}

const updateTrigger = (rule: RuleDto, trigger: any, user: string, version: Version, now?: DateTime) =>
    rule.with({
        trigger,
        triggerType: trigger.triggerType,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const updateAction = (rule: RuleDto, action: any, user: string, version: Version, now?: DateTime) =>
    rule.with({
        action,
        actionType: action.actionType,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const setEnabled = (rule: RuleDto, isEnabled: boolean, user: string, version: Version, now?: DateTime) =>
    rule.with({
        isEnabled,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

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
    DateTime,
    DialogService,
    ImmutableArray,
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
        this.changes.map(x => x.rules)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

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

        return this.rulesService.getRules(this.appName)
            .do(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Rules reloaded.');
                }

                this.next(s => {
                    const rules = ImmutableArray.of(dtos);

                    return { ...s, rules, isLoaded: true };
                });
            })
            .notify(this.dialogs);
    }

    public create(request: CreateRuleDto, now?: DateTime): Observable<any> {
        return this.rulesService.postRule(this.appName, request, this.user, now || DateTime.now())
            .do(dto => {
                this.next(s => {
                    const rules = s.rules.push(dto);

                    return { ...s, rules };
                });
            })
            .notify(this.dialogs);
    }

    public delete(rule: RuleDto): Observable<any> {
        return this.rulesService.deleteRule(this.appName, rule.id, rule.version)
            .do(dto => {
                this.next(s => {
                    const rules = s.rules.removeAll(x => x.id === rule.id);

                    return { ...s, rules };
                });
            })
            .notify(this.dialogs);
    }

    public updateAction(rule: RuleDto, action: any, now?: DateTime): Observable<any> {
        return this.rulesService.putRule(this.appName, rule.id, new UpdateRuleDto(null, action), rule.version)
            .do(dto => {
                this.replaceRule(updateAction(rule, action, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public updateTrigger(rule: RuleDto, trigger: any, now?: DateTime): Observable<any> {
        return this.rulesService.putRule(this.appName, rule.id, new UpdateRuleDto(trigger, null), rule.version)
            .do(dto => {
                this.replaceRule(updateTrigger(rule, trigger, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public enable(rule: RuleDto, now?: DateTime): Observable<any> {
        return this.rulesService.enableRule(this.appName, rule.id, rule.version)
            .do(dto => {
                this.replaceRule(setEnabled(rule, true, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public disable(rule: RuleDto, now?: DateTime): Observable<any> {
        return this.rulesService.disableRule(this.appName, rule.id, rule.version)
            .do(dto => {
                this.replaceRule(setEnabled(rule, false, this.user, dto.version, now));
            })
            .notify(this.dialogs);
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
    new RuleDto(
        rule.id,
        rule.createdBy, user,
        rule.created, now || DateTime.now(),
        version,
        rule.isEnabled,
        trigger,
        trigger.triggerType,
        rule.action,
        rule.action.actionType);

const updateAction = (rule: RuleDto, action: any, user: string, version: Version, now?: DateTime) =>
    new RuleDto(
        rule.id,
        rule.createdBy, user,
        rule.created, now || DateTime.now(),
        version,
        rule.isEnabled,
        rule.trigger,
        rule.trigger.triggerType,
        action,
        action.actionType);

const setEnabled = (rule: RuleDto, isEnabled: boolean, user: string, version: Version, now?: DateTime) =>
    new RuleDto(
        rule.id,
        rule.createdBy, user,
        rule.created, now || DateTime.now(),
        version,
        isEnabled,
        rule.trigger,
        rule.triggerType,
        rule.action,
        rule.actionType);

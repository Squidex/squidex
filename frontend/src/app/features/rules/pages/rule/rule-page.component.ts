/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BehaviorSubject, debounceTime, distinctUntilChanged, filter, map, switchMap } from 'rxjs';
import { ALL_TRIGGERS, ConfirmClickDirective, DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicRuleDto, DynamicUpdateRuleDto, ErrorDto, FlowStepDefinitionDto, FlowView, IDynamicFlowStepDefinitionDto, LayoutComponent, MessageBus, ModalDirective, Mutable, RuleElementDto, RulesService, RulesState, RuleTriggerDto, SchemasState, SidebarMenuDirective, Subscriptions, TitleComponent, ToggleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';
import { RuleConfigured } from '../messages';
import { TriggerDialogComponent } from './trigger-dialog.component';
import { FlowStepAdd, FlowStepRemove } from './types';
import { StepDialogComponent } from "./step-dialog.component";

type Snapshot = {
    flow: FlowView;
    name?: string;
    isEditable: boolean;
    isEnabled: boolean;
    trigger?: RuleTriggerDto | null;
};

@Component({
    standalone: true,
    selector: 'sqx-rule-page',
    styleUrls: ['./rule-page.component.scss'],
    templateUrl: './rule-page.component.html',
    imports: [
    AsyncPipe,
    ConfirmClickDirective,
    FormsModule,
    LayoutComponent,
    ModalDirective,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    SidebarMenuDirective,
    TitleComponent,
    ToggleComponent,
    TooltipDirective,
    TourHintDirective,
    TourStepDirective,
    TranslatePipe,
    TriggerDialogComponent,
    StepDialogComponent
],
})
export class RulePageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public supportedTriggers = ALL_TRIGGERS;
    public supportedActions: { [name: string]: RuleElementDto } = {};

    public error?: ErrorDto;

    public targetTrigger?: RuleTriggerDto;
    public targetStep?: { step: FlowStepDefinitionDto; target: string | FlowStepAdd };

    public rule?: DynamicRuleDto | null;

    public readonly editableRule = new BehaviorSubject<Snapshot>({
        flow: new FlowView(new DynamicFlowDefinitionDto()),
        isEditable: true,
        isEnabled: true,
    });

    public scriptCompletions =
        this.editableRule.pipe(
            map(x => x.trigger?.triggerType!),
            filter(x => !!x),
            distinctUntilChanged(),
            switchMap(x => this.rulesService.getCompletions(this.rulesState.appName, x)));

    public get isManual() {
        return this.rule?.trigger?.triggerType === 'Manual';
    }

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
    }

    public ngOnInit() {
        this.rulesService.getActions()
            .subscribe(actions => {
                this.supportedActions = actions;
            });

        this.subscriptions.add(
            this.editableRule.pipe(debounceTime(100))
                .subscribe(() => {
                    this.publishState();
                }));

        this.subscriptions.add(
            this.rulesState.selectedRule
                .subscribe(rule => {
                    this.initFromRule(rule);
                }));

        this.schemasState.loadIfNotLoaded();
    }

    private initFromRule(rule?: DynamicRuleDto | null) {
        this.rule = rule;

        if (rule) {
            this.editableRule.next({
                flow: new FlowView(rule.flow),
                name: rule.name,
                isEditable: rule.canUpdate,
                isEnabled: rule.canEnable,
                trigger: rule.trigger,
            });
        }
    }

    public save() {
        const editableRule = this.editableRule.value;
        if (!editableRule?.isEditable || !editableRule.trigger) {
            return;
        }

        const { flow, isEnabled, trigger } = this.editableRule.value;

        if (this.rule) {
            const request = new DynamicUpdateRuleDto({ flow, isEnabled: this.isEnabled });

            this.rulesState.update(this.rule, request)
                .subscribe({
                    error: error => {
                        this.error = error;
                    },
                });
        } else {
            const request = new DynamicCreateRuleDto({ trigger, flow });

            this.rulesState.create(request)
                .subscribe({
                    error: error => {
                        this.error = error;
                    },
                });
        }
    }

    private publishState() {
        const editableRule = this.editableRule.value;
        if (!editableRule.trigger) {
            return;
        }

        this.messageBus.emit(new RuleConfigured(
            editableRule.trigger,
            editableRule.flow));
    }

    public changeStep(values: Mutable<IDynamicFlowStepDefinitionDto>) {
        const target = this.targetStep!.target;
        this.update(s => ({
            ...s,
            flow: Types.isString(target) ?
                s.flow.update(target, values) :
                s.flow.add(values, target.afterId, target.parentId, target.branchIndex),
        }));
    }

    public changeTrigger(trigger: RuleTriggerDto) {
        this.update(s => ({ ...s, trigger }));
    }

    public rename(name: string) {
        this.update(s => ({ ...s, name }));
    }

    public removeStep(event: FlowStepRemove) {
        this.update(s => ({ ...s, flow: s.flow.remove(event.id, event.parentId, event.branchIndex) }));
    }

    public cancel() {
        this.update(s => s);
    }

    private update(action: (value: Snapshot) => Snapshot) {
        this.editableRule.next(action(this.editableRule.value));
        this.targetStep = undefined;
        this.targetTrigger = undefined;
    }

    public trigger() {
        this.rulesState.trigger(this.rule!);
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}

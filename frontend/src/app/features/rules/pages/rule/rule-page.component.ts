/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, LowerCasePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BehaviorSubject, debounceTime, map, switchMap } from 'rxjs';
import { ALL_TRIGGERS, ConfirmClickDirective, defined, DialogModel, DialogService, DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicFlowStepDefinitionDto, DynamicRuleDto, DynamicUpdateRuleDto, ErrorDto, FlowView, IDynamicFlowStepDefinitionDto, LayoutComponent, MessageBus, ModalDirective, Mutable, RuleElementDto, RulesService, RulesState, RuleTriggerDto, SchemasState, SidebarMenuDirective, Subscriptions, TitleComponent, ToggleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { RuleConfigured } from '../messages';
import { BranchComponent } from './branch.component';
import { StepDialogComponent } from './step-dialog.component';
import { TriggerDialogComponent } from './trigger-dialog.component';
import { FlowStepAdd, FlowStepRemove, FlowStepUpdate } from './types';

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
        BranchComponent,
        ConfirmClickDirective,
        FormsModule,
        LayoutComponent,
        LowerCasePipe,
        ModalDirective,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        RuleElementComponent,
        SidebarMenuDirective,
        StepDialogComponent,
        TitleComponent,
        ToggleComponent,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
        TriggerDialogComponent,
    ],
})
export class RulePageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public availableTriggers = ALL_TRIGGERS;
    public availableSteps: { [name: string]: RuleElementDto } = {};

    public error?: ErrorDto;

    public stepDialog = new DialogModel();
    public stepToUpsert?: { step?: DynamicFlowStepDefinitionDto; target: string | FlowStepAdd };

    public triggerToEdit?: RuleTriggerDto;
    public triggerDialog = new DialogModel();

    public rule?: DynamicRuleDto | null;

    public readonly editableRule = new BehaviorSubject<Snapshot>({
        flow: new FlowView(new DynamicFlowDefinitionDto({ initialStepId: null!, steps: {} })),
        isEditable: true,
        isEnabled: true,
    });

    public scriptCompletions =
        this.editableRule.pipe(
            map(x => x.trigger?.triggerType!),
            defined(),
            switchMap(x => this.rulesService.getCompletions(this.rulesState.appName, x)));

    public get isManual() {
        return this.rule?.trigger?.triggerType === 'Manual';
    }

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
        private readonly dialogs: DialogService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
    }

    public ngOnInit() {
        this.rulesService.getSteps()
            .subscribe(steps => {
                this.availableSteps = steps;
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
                isEnabled: rule.isEnabled,
                trigger: rule.trigger,
            });
        }
    }

    public save() {
        const { flow, isEditable, isEnabled, name, trigger } = this.editableRule.value;
        if (!isEditable || !trigger || !flow?.dto.steps || Object.entries(flow.dto.steps).length === 0) {
            return;
        }

        if (this.rule) {
            const request = new DynamicUpdateRuleDto({ flow: flow.dto, isEnabled, name, trigger });

            this.rulesState.update(this.rule, request)
                .subscribe({
                    complete: () => {
                        this.dialogs.notifyInfo('i18n:rules.updated');
                    },
                    error: error => {
                        this.error = error;
                    },
                });
        } else {
            const request = new DynamicCreateRuleDto({ flow: flow.dto, isEnabled, name, trigger });

            this.rulesState.create(request)
                .subscribe({
                    next: rule => {
                        this.router.navigate(['../', rule.id], { relativeTo: this.route, replaceUrl: true });
                    },
                    complete: () => {
                        this.dialogs.notifyInfo('i18n:rules.created');
                    },
                    error: error => {
                        this.error = error;
                    },
                });
        }
    }

    private publishState() {
        const { trigger, flow } = this.editableRule.value;

        if (trigger) {
            this.messageBus.emit(new RuleConfigured(trigger, flow.dto));
        }
    }

    public changeStep(values: Mutable<IDynamicFlowStepDefinitionDto>) {
        const target = this.stepToUpsert!.target;
        this.update(s => ({
            ...s,
            flow: Types.isString(target) ?
                s.flow.update(target, values) :
                s.flow.add(values, target.afterId, target.parentId, target.branchIndex),
        }));
    }

    public startUpdateTrigger(update: RuleTriggerDto) {
        const metadata = this.availableTriggers[update.triggerType];
        if (!metadata || !metadata.hasProperties) {
            return;
        }

        this.triggerToEdit = update;
        this.triggerDialog.show();
    }

    public startUpdateStep(update: FlowStepUpdate) {
        this.stepToUpsert = { target: update.id, step: update.values };
        this.stepDialog.show();
    }

    public startAddStep(target: FlowStepAdd) {
        this.stepToUpsert = { target };
        this.stepDialog.show();
    }

    public changeTrigger(trigger: RuleTriggerDto) {
        this.update(s => ({ ...s, trigger }));
    }

    public changeEnabled(isEnabled: boolean) {
        this.update(s => ({ ...s, isEnabled }));
    }

    public rename(name: string) {
        this.update(s => ({ ...s, name }));
    }

    public removeTrigger() {
        this.update(s => ({ ...s, trigger: undefined }));
    }

    public removeStep(event: FlowStepRemove) {
        this.update(s => ({ ...s, flow: s.flow.remove(event.id, event.parentId, event.branchIndex) }));
    }

    public cancel() {
        this.update(s => s);
    }

    private update(action: (value: Snapshot) => Snapshot) {
        this.editableRule.next(action(this.editableRule.value));
        this.stepToUpsert = undefined;
        this.stepDialog.hide();
        this.triggerToEdit = undefined;
        this.triggerDialog.hide();
    }

    public trigger() {
        this.rulesState.trigger(this.rule!);
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}

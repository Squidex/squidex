/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LowerCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ALL_TRIGGERS, ConfirmClickDirective, DialogModel, DialogService, DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicFlowStepDefinitionDto, DynamicRuleDto, DynamicUpdateRuleDto, ErrorDto, FlowView, IDynamicFlowStepDefinitionDto, LayoutComponent, MessageBus, ModalDirective, Mutable, RuleElementDto, RulesService, RulesState, RuleTriggerDto, SchemasState, ScriptCompletions, SidebarMenuDirective, TitleComponent, ToggleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';
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
    selector: 'sqx-rule-page',
    styleUrls: ['./rule-page.component.scss'],
    templateUrl: './rule-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        BranchComponent,
        ConfirmClickDirective,
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
export class RulePageComponent {
    private readonly dialogs = inject(DialogService);
    private readonly messageBus = inject(MessageBus);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly rulesService = inject(RulesService);
    private readonly schemasState = inject(SchemasState);

    protected readonly rulesState = inject(RulesState);

    protected readonly error = signal<ErrorDto | undefined>(undefined);

    protected readonly stepToUpsert = signal<{ step?: DynamicFlowStepDefinitionDto; target: string | FlowStepAdd } | undefined>(undefined);

    protected readonly triggerToEdit = signal<RuleTriggerDto | undefined>(undefined);

    protected readonly rule = signal<DynamicRuleDto | null | undefined>(undefined);

    protected readonly editableRule = signal<Snapshot>({
        flow: new FlowView(new DynamicFlowDefinitionDto({ initialStepId: null!, steps: {} })),
        isEditable: true,
        isEnabled: true,
    });

    protected readonly isManual = computed(() => {
        return this.rule()?.trigger?.triggerType === 'Manual';
    });

    protected readonly scriptCompletions = rxResource<ScriptCompletions | null, string | undefined>({
        params: () => this.editableRule().trigger?.triggerType || undefined,
        stream: ({ params }) => this.rulesService.getCompletions(this.rulesState.appName, params),
        defaultValue: null,
    });

    protected readonly availableSteps = rxResource({
        stream: () => this.rulesService.getSteps(),
        defaultValue: {} as { [name: string]: RuleElementDto },
    });

    protected readonly availableTriggers = ALL_TRIGGERS;

    protected readonly stepDialog = new DialogModel();

    protected readonly triggerDialog = new DialogModel();

    constructor() {
        effect(onCleanup => {
            const { trigger, flow } = this.editableRule();

            const timer = setTimeout(() => {
                if (trigger) {
                    this.messageBus.emit(new RuleConfigured(trigger, flow.dto));
                }
            }, 100);

            onCleanup(() => clearTimeout(timer));
        });

        effect(() => {
            this.initFromRule(this.rulesState.selectedRule());
        });

        this.schemasState.loadIfNotLoaded();
    }

    private initFromRule(rule?: DynamicRuleDto | null) {
        this.rule.set(rule);

        if (rule) {
            this.editableRule.set({
                flow: new FlowView(rule.flow),
                name: rule.name,
                isEditable: rule.canUpdate,
                isEnabled: rule.isEnabled,
                trigger: rule.trigger,
            });
        }
    }

    protected save() {
        const { flow, isEditable, isEnabled, name, trigger } = this.editableRule();
        if (!isEditable || !trigger || !flow?.dto.steps || Object.entries(flow.dto.steps).length === 0) {
            return;
        }

        const rule = this.rule();
        if (rule) {
            const request = new DynamicUpdateRuleDto({ flow: flow.dto, isEnabled, name, trigger });

            this.rulesState.update(rule, request)
                .subscribe({
                    complete: () => {
                        this.dialogs.notifyInfo('i18n:rules.updated');
                    },
                    error: error => {
                        this.error.set(error);
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
                        this.error.set(error);
                    },
                });
        }
    }

    protected changeStep(values: Mutable<IDynamicFlowStepDefinitionDto>) {
        const target = this.stepToUpsert()!.target;
        this.update(s => ({
            ...s,
            flow: Types.isString(target) ?
                s.flow.update(target, values) :
                s.flow.add(values, target.afterId, target.parentId, target.branchIndex),
        }));
    }

    protected startUpdateTrigger(update: RuleTriggerDto) {
        const metadata = this.availableTriggers[update.triggerType];
        if (!metadata || !metadata.hasProperties) {
            return;
        }

        this.triggerToEdit.set(update);
        this.triggerDialog.show();
    }

    protected startUpdateStep(update: FlowStepUpdate) {
        this.stepToUpsert.set({ target: update.id, step: update.values });
        this.stepDialog.show();
    }

    protected startAddStep(target: FlowStepAdd) {
        this.stepToUpsert.set({ target });
        this.stepDialog.show();
    }

    protected changeTrigger(trigger: RuleTriggerDto) {
        this.update(s => ({ ...s, trigger }));
    }

    protected changeEnabled(isEnabled: boolean) {
        this.update(s => ({ ...s, isEnabled }));
    }

    protected rename(name: string) {
        this.update(s => ({ ...s, name }));
    }

    protected removeTrigger() {
        this.update(s => ({ ...s, trigger: undefined }));
    }

    protected removeStep(event: FlowStepRemove) {
        this.update(s => ({ ...s, flow: s.flow.remove(event.id, event.parentId, event.branchIndex) }));
    }

    protected cancel() {
        this.update(s => s);
    }

    private update(action: (value: Snapshot) => Snapshot) {
        this.editableRule.update(action);
        this.stepToUpsert.set(undefined);
        this.stepDialog.hide();
        this.triggerToEdit.set(undefined);
        this.triggerDialog.hide();
    }

    protected trigger() {
        this.rulesState.trigger(this.rule()!);
    }

    protected back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}

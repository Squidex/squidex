/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, LowerCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map, switchMap } from 'rxjs';
import { ALL_TRIGGERS, ConfirmClickDirective, defined, DialogModel, DialogService, DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicFlowStepDefinitionDto, DynamicRuleDto, DynamicUpdateRuleDto, ErrorDto, FlowView, IDynamicFlowStepDefinitionDto, LayoutComponent, MessageBus, ModalDirective, Mutable, RuleElementDto, RulesService, RulesState, RuleTriggerDto, SchemasState, SidebarMenuDirective, TitleComponent, ToggleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types } from '@app/shared';
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
export class RulePageComponent {
    public readonly availableTriggers = ALL_TRIGGERS;
    public readonly availableSteps;

    public readonly error = signal<ErrorDto | undefined>(undefined);

    public readonly stepDialog = new DialogModel();
    public readonly stepToUpsert = signal<{ step?: DynamicFlowStepDefinitionDto; target: string | FlowStepAdd } | undefined>(undefined);

    public readonly triggerToEdit = signal<RuleTriggerDto | undefined>(undefined);
    public readonly triggerDialog = new DialogModel();

    public readonly rule = signal<DynamicRuleDto | null | undefined>(undefined);

    public readonly editableRule = signal<Snapshot>({
        flow: new FlowView(new DynamicFlowDefinitionDto({ initialStepId: null!, steps: {} })),
        isEditable: true,
        isEnabled: true,
    });

    public readonly scriptCompletions;

    public readonly isManual = computed(() => {
        return this.rule()?.trigger?.triggerType === 'Manual';
    });

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
        private readonly dialogs: DialogService,
        messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
        this.availableSteps = toSignal(rulesService.getSteps(), { initialValue: {} as { [name: string]: RuleElementDto } });

        this.scriptCompletions = toSignal(
            toObservable(this.editableRule).pipe(
                map(x => x.trigger?.triggerType!),
                defined(),
                switchMap(x => rulesService.getCompletions(rulesState.appName, x))),
            { initialValue: null });

        effect(onCleanup => {
            const { trigger, flow } = this.editableRule();

            const timer = setTimeout(() => {
                if (trigger) {
                    messageBus.emit(new RuleConfigured(trigger, flow.dto));
                }
            }, 100);

            onCleanup(() => clearTimeout(timer));
        });

        effect(onCleanup => {
            const subscription = rulesState.selectedRule
                .subscribe(rule => {
                    this.initFromRule(rule);
                });

            onCleanup(() => subscription.unsubscribe());
        });

        schemasState.loadIfNotLoaded();
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

    public save() {
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

    public changeStep(values: Mutable<IDynamicFlowStepDefinitionDto>) {
        const target = this.stepToUpsert()!.target;
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

        this.triggerToEdit.set(update);
        this.triggerDialog.show();
    }

    public startUpdateStep(update: FlowStepUpdate) {
        this.stepToUpsert.set({ target: update.id, step: update.values });
        this.stepDialog.show();
    }

    public startAddStep(target: FlowStepAdd) {
        this.stepToUpsert.set({ target });
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
        this.editableRule.update(action);
        this.stepToUpsert.set(undefined);
        this.stepDialog.hide();
        this.triggerToEdit.set(undefined);
        this.triggerDialog.hide();
    }

    public trigger() {
        this.rulesState.trigger(this.rule()!);
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}

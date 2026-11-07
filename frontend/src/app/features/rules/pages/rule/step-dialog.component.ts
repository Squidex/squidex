/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LowerCasePipe } from '@angular/common';
import { booleanAttribute, Component, effect, inject, Injector, input, linkedSignal, output, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AppsState, CodeEditorComponent, DynamicFlowStepDefinitionDto, EntriesPipe, FormErrorComponent, FormHintComponent, FormRowComponent, MarkdownDirective, ModalDialogComponent, RuleElementDto, RulesService, ScriptCompletions, SignalSelectDirective, StepForm, TranslatePipe } from '@app/shared';
import { BranchesInputComponent } from '../../shared/actions/branches-input.component';
import { FormattableInputComponent } from '../../shared/actions/formattable-input.component';
import { RuleElementComponent } from '../../shared/rule-element.component';

@Component({
    selector: 'sqx-step-dialog',
    styleUrls: ['./step-dialog.component.scss'],
    templateUrl: './step-dialog.component.html',
    imports: [
        BranchesInputComponent,
        CodeEditorComponent,
        EntriesPipe,
        FormattableInputComponent,
        FormErrorComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LowerCasePipe,
        MarkdownDirective,
        ModalDialogComponent,
        RuleElementComponent,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class StepDialogComponent {
    private readonly appsState = inject(AppsState);
    private readonly injector = inject(Injector);
    private readonly rulesService = inject(RulesService);

    /** The step to edit, or undefined to add a new step. */
    public readonly stepDefinition = input.required<DynamicFlowStepDefinitionDto | undefined>();

    /** Allows the user to change and save the step. */
    public readonly isEditable = input.required({ transform: booleanAttribute });

    /** The available steps by type, which the user can choose from. */
    public readonly availableSteps = input.required<{ [name: string]: RuleElementDto }>();

    /** The suggestions for autocompletion in script and code properties. */
    public readonly scriptCompletions = input.required<ScriptCompletions | null>();

    /** Emits when the user removes the step. */
    public readonly remove = output();

    /** Emits when the dialog is closed without saving. */
    public readonly dialogClose = output();

    /** Emits the validated step when the user saves it. */
    public readonly dialogSave = output<DynamicFlowStepDefinitionDto>();

    protected readonly currentStep = signal<StepForm | undefined>(undefined);

    protected readonly stepName = linkedSignal(() => {
        return this.stepDefinition()?.name;
    });

    protected readonly stepIgnoreError = linkedSignal(() => {
        return this.stepDefinition()?.ignoreError || false;
    });

    constructor() {
        effect(() => {
            const stepDefinition = this.stepDefinition();
            if (!stepDefinition) {
                return;
            }

            untracked(() => {
                this.selectStep(stepDefinition.step.stepType, stepDefinition.step);
            });
        });
    }

    protected selectStep(type: string, values?: any) {
        if (this.currentStep()?.stepType !== type) {
            const element = this.availableSteps()[type];

            const currentStep = new StepForm(element, type, this.injector);
            currentStep.setEnabled(this.isEditable());

            this.currentStep.set(currentStep);
        }

        if (values) {
            this.currentStep()?.load(values || {});
        }
    }

    protected save() {
        const currentStep = this.currentStep();
        if (!this.isEditable() || !currentStep) {
            return;
        }

        const values = currentStep.submit();
        if (!values) {
            return;
        }

        this.rulesService.validateStep(this.appsState.appName, values)
            .subscribe({
                error: error =>{
                    currentStep.submitFailed(error);
                },
                complete: () => {
                    this.dialogSave.emit(new DynamicFlowStepDefinitionDto({ name: this.stepName(), ignoreError: this.stepIgnoreError(), step: values }));
                },
            });

    }
}

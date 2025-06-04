/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, LowerCasePipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppsState, CodeEditorComponent, ControlErrorsComponent, DynamicFlowStepDefinitionDto, EntriesPipe, FormErrorComponent, FormHintComponent, MarkdownDirective, ModalDialogComponent, RuleElementDto, RulesService, ScriptCompletions, StepForm, TranslatePipe, TypedSimpleChanges } from '@app/shared';
import { BranchesInputComponent } from '../../shared/actions/branches-input.component';
import { FormattableInputComponent } from '../../shared/actions/formattable-input.component';
import { RuleElementComponent } from '../../shared/rule-element.component';

@Component({
    selector: 'sqx-step-dialog',
    styleUrls: ['./step-dialog.component.scss'],
    templateUrl: './step-dialog.component.html',
    imports: [
        AsyncPipe,
        BranchesInputComponent,
        CodeEditorComponent,
        ControlErrorsComponent,
        EntriesPipe,
        FormattableInputComponent,
        FormErrorComponent,
        FormHintComponent,
        FormsModule,
        LowerCasePipe,
        MarkdownDirective,
        ModalDialogComponent,
        ReactiveFormsModule,
        RuleElementComponent,
        TranslatePipe,
    ]
})
export class StepDialogComponent {
    @Input({ required: true })
    public stepDefinition?: DynamicFlowStepDefinitionDto;

    @Input({ required: true, transform: booleanAttribute })
    public isEditable = true;

    @Input({ required: true })
    public availableSteps: { [name: string]: RuleElementDto } = {};

    @Input({ required: true })
    public scriptCompletions: ScriptCompletions | null = [];

    @Output()
    public remove = new EventEmitter();

    @Output()
    public dialogClose = new EventEmitter();

    @Output()
    public dialogSave = new EventEmitter<DynamicFlowStepDefinitionDto>();

    public currentStep?: StepForm;

    public stepName?: string;
    public stepIgnoreError = false;

    constructor(
        public appsState: AppsState,
        private rulesService: RulesService,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.stepDefinition && this.stepDefinition) {
            this.selectStep(this.stepDefinition.step.stepType, this.stepDefinition.step);

            this.stepIgnoreError = this.stepDefinition.ignoreError || false;
            this.stepName = this.stepDefinition?.name;
        }
    }

    public selectStep(type: string, values?: any) {
        if (this.currentStep?.stepType !== type) {
            const element = this.availableSteps[type];

            this.currentStep = new StepForm(element, type);
            this.currentStep.setEnabled(this.isEditable);
        }

        if (values) {
            this.currentStep?.load(values || {});
        }
    }

    public save() {
        if (!this.isEditable || !this.currentStep) {
            return;
        }

        const values = this.currentStep.submit();
        if (!values) {
            return;
        }

        this.rulesService.validateStep(this.appsState.appName, values)
            .subscribe({
                error: error =>{
                    this.currentStep?.submitFailed(error);
                },
                complete: () => {
                    this.dialogSave.emit(new DynamicFlowStepDefinitionDto({ name: this.stepName, ignoreError: this.stepIgnoreError, step: values }));
                },
            });

    }
}
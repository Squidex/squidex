/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LowerCasePipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeEditorComponent, ControlErrorsComponent, DynamicFlowStepDefinitionDto, FormHintComponent, KeysPipe, MarkdownDirective, ModalDialogComponent, RuleElementDto, ScriptCompletions, StepForm, TranslatePipe, TypedSimpleChanges } from '@app/shared';
import { BranchesInputComponent } from '../../shared/actions/branches-input.component';
import { FormattableInputComponent } from '../../shared/actions/formattable-input.component';
import { RuleElementComponent } from '../../shared/rule-element.component';

@Component({
    standalone: true,
    selector: 'sqx-step-dialog',
    styleUrls: ['./step-dialog.component.scss'],
    templateUrl: './step-dialog.component.html',
    imports: [
        BranchesInputComponent,
        CodeEditorComponent,
        ControlErrorsComponent,
        FormattableInputComponent,
        FormHintComponent,
        FormsModule,
        KeysPipe,
        LowerCasePipe,
        MarkdownDirective,
        ModalDialogComponent,
        ReactiveFormsModule,
        RuleElementComponent,
        TranslatePipe,
    ],
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

    public stepIgnore = false;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.stepDefinition && this.stepDefinition) {
            this.selectStep(this.stepDefinition.step.stepType, this.stepDefinition.step);

            this.stepIgnore = this.stepDefinition.ignoreError || false;
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

        const step = this.currentStep.submit();
        if (!step) {
            return;
        }

        this.dialogSave.emit(new DynamicFlowStepDefinitionDto({ name: this.stepName, ignoreError: this.stepIgnore, step }));
    }
}
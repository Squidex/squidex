/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LowerCasePipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActionForm, CodeEditorComponent, ControlErrorsComponent, DynamicFlowStepDefinitionDto, FormHintComponent, KeysPipe, MarkdownDirective, ModalDialogComponent, RuleElementDto, ScriptCompletions, TranslatePipe } from '@app/shared';
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
export class StepDialogComponent implements OnInit {
    @Input({ required: true })
    public stepDefinition?: DynamicFlowStepDefinitionDto;

    @Input({ required: true, transform: booleanAttribute })
    public isEditable = true;

    @Input({ required: true })
    public supportedSteps: { [name: string]: RuleElementDto } = {};

    @Input({ required: true })
    public scriptCompletions: ScriptCompletions | null = [];

    @Output()
    public dialogClose = new EventEmitter();

    @Output()
    public dialogSaved = new EventEmitter<DynamicFlowStepDefinitionDto>();

    public currentAction?: ActionForm;

    public ignoreError = false;

    public ngOnInit() {
        if (this.stepDefinition) {
            this.selectStep(this.stepDefinition.step.stepType, this.stepDefinition.step);

            this.ignoreError = this.stepDefinition.ignoreError || false;
        }
    }

    public selectStep(type: string, values?: any) {
        if (this.currentAction?.actionType !== type) {
            const element = this.supportedSteps[type];

            this.currentAction = new ActionForm(element, type);
            this.currentAction.setEnabled(this.isEditable);
        }

        if (values) {
            this.currentAction?.load(values || {});
        }
    }

    public save() {
        if (!this.isEditable || !this.currentAction) {
            return;
        }

        const values = this.currentAction.submit();
        if (!values) {
            return;
        }

        this.dialogSaved.emit(new DynamicFlowStepDefinitionDto({ ignoreError: this.ignoreError, step: values }));
    }
}
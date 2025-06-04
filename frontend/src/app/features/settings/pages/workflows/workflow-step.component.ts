/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ColorPickerComponent, DropdownComponent, EditableTitleComponent, TagEditorComponent, TranslatePipe, TypedSimpleChanges, WorkflowStepValues, WorkflowStepView, WorkflowTransitionView, WorkflowView } from '@app/shared';
import { WorkflowTransitionComponent } from './workflow-transition.component';

@Component({
    selector: 'sqx-workflow-step',
    styleUrls: ['./workflow-step.component.scss'],
    templateUrl: './workflow-step.component.html',
    imports: [
        ColorPickerComponent,
        DropdownComponent,
        EditableTitleComponent,
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
        WorkflowTransitionComponent,
    ]
})
export class WorkflowStepComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    @Output()
    public update = new EventEmitter<WorkflowView>();

    @Input({ required: true })
    public workflow!: WorkflowView;

    @Input({ required: true })
    public step!: WorkflowStepView;

    @Input({ required: true })
    public roles!: ReadonlyArray<string>;

    @Input({ transform: booleanAttribute })
    public disabled?: boolean | null;

    public openSteps!: ReadonlyArray<WorkflowStepView>;
    public openStep!: WorkflowStepView;

    public transitions!: ReadonlyArray<WorkflowTransitionView>;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.workflow || changes.step) {
            this.openSteps = this.workflow.getOpenSteps(this.step);
            this.openStep = this.openSteps[0];

            this.transitions = this.workflow.getTransitions(this.step);
        }
    }

    public changeColor(color: string) {
        this.change({ color });
    }

    public changeValidate(validate: boolean) {
        this.change({ validate });
    }

    public changeNoUpdate(noUpdate: boolean) {
        this.change({ noUpdate });
    }

    public changeNoUpdateExpression(noUpdateExpression?: string) {
        this.change({ noUpdateExpression });
    }

    public changeNoUpdateRoles(noUpdateRoles?: string[]) {
        this.change({ noUpdateRoles });
    }

    public addTransition(to: string) {
        const { name } = this.step;
        this.update.emit(this.workflow.setTransition(name, to));
    }

    public remove() {
        const { name } = this.step;
        this.update.emit(this.workflow.removeStep(name));
    }

    public removeTransition(to: string) {
        const { name } = this.step;
        this.update.emit(this.workflow.removeTransition(name, to));
    }

    public makeInitial() {
        const { name } = this.step;
        this.update.emit(this.workflow.setInitial(name));
    }

    public changeName(newName: string) {
        const { name } = this.step;
        this.update.emit(this.workflow.renameStep(name, newName));
    }

    private change(changes: Partial<WorkflowStepValues>) {
        const { name } = this.step;
        this.update.emit(this.workflow.setStep(name, changes));
    }
}

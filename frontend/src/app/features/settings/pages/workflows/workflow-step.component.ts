/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ColorPickerComponent, DropdownComponent, EditableTitleComponent, TagEditorComponent, TranslatePipe, WorkflowStepValues, WorkflowStepView, WorkflowView } from '@app/shared';
import { WorkflowTransitionComponent } from './workflow-transition.component';

@Component({
    selector: 'sqx-workflow-step',
    styleUrls: ['./workflow-step.component.scss'],
    templateUrl: './workflow-step.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ColorPickerComponent,
        DropdownComponent,
        EditableTitleComponent,
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
        WorkflowTransitionComponent,
    ],
})
export class WorkflowStepComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    /** Emits the updated workflow when the step or one of its transitions is changed. */
    public readonly update = output<WorkflowView>();

    /** The workflow the step belongs to. */
    public readonly workflow = input.required<WorkflowView>();

    /** The step to show and edit. */
    public readonly step = input.required<WorkflowStepView>();

    /** The roles that can be selected for the step and its transitions. */
    public readonly roles = input.required<ReadonlyArray<string>>();

    /** Prevents the step from being changed. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    public readonly openSteps = computed(() => {
        return this.workflow().getOpenSteps(this.step());
    });

    public readonly openStep = linkedSignal(() => this.openSteps()[0]);

    public readonly transitions = computed(() => {
        return this.workflow().getTransitions(this.step());
    });

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
        const { name } = this.step();
        this.update.emit(this.workflow().setTransition(name, to));
    }

    public remove() {
        const { name } = this.step();
        this.update.emit(this.workflow().removeStep(name));
    }

    public removeTransition(to: string) {
        const { name } = this.step();
        this.update.emit(this.workflow().removeTransition(name, to));
    }

    public makeInitial() {
        const { name } = this.step();
        this.update.emit(this.workflow().setInitial(name));
    }

    public changeName(newName: string) {
        const { name } = this.step();
        this.update.emit(this.workflow().renameStep(name, newName));
    }

    private change(changes: Partial<WorkflowStepValues>) {
        const { name } = this.step();
        this.update.emit(this.workflow().setStep(name, changes));
    }
}

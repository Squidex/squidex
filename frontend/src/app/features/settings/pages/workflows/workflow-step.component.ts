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
    protected readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

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

    protected readonly openSteps = computed(() => {
        return this.workflow().getOpenSteps(this.step());
    });

    protected readonly openStep = linkedSignal(() => this.openSteps()[0]);

    protected readonly transitions = computed(() => {
        return this.workflow().getTransitions(this.step());
    });

    protected changeColor(color: string) {
        this.change({ color });
    }

    protected changeValidate(validate: boolean) {
        this.change({ validate });
    }

    protected changeNoUpdate(noUpdate: boolean) {
        this.change({ noUpdate });
    }

    protected changeNoUpdateExpression(noUpdateExpression?: string) {
        this.change({ noUpdateExpression });
    }

    protected changeNoUpdateRoles(noUpdateRoles?: string[]) {
        this.change({ noUpdateRoles });
    }

    protected addTransition(to: string) {
        const { name } = this.step();
        this.update.emit(this.workflow().setTransition(name, to));
    }

    protected remove() {
        const { name } = this.step();
        this.update.emit(this.workflow().removeStep(name));
    }

    protected removeTransition(to: string) {
        const { name } = this.step();
        this.update.emit(this.workflow().removeTransition(name, to));
    }

    protected makeInitial() {
        const { name } = this.step();
        this.update.emit(this.workflow().setInitial(name));
    }

    protected changeName(newName: string) {
        const { name } = this.step();
        this.update.emit(this.workflow().renameStep(name, newName));
    }

    private change(changes: Partial<WorkflowStepValues>) {
        const { name } = this.step();
        this.update.emit(this.workflow().setStep(name, changes));
    }
}

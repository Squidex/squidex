/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { ControlErrorsComponent, StepForm, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-branches-input',
    styleUrls: ['./branches-input.component.scss'],
    templateUrl: './branches-input.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormField,
        TranslatePipe,
    ],
})
export class BranchesInputComponent {
    /** The form of the step that contains the branches. */
    public readonly stepForm = input.required<StepForm>();

    /** The name of the step property with the branches. */
    public readonly property = input.required<string>();

    /** Allows the user to add and remove branches. */
    public readonly isEditable = input.required({ transform: booleanAttribute });

    protected readonly branches = computed(() => {
        return this.stepForm().branches(this.property());
    });

    protected add() {
        this.stepForm().addBranch(this.property());
    }

    protected remove(index: number) {
        this.stepForm().removeBranch(this.property(), index);
    }
}

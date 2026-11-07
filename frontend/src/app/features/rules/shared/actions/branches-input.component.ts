/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, ExtendedFormGroup, TemplatedFormArray, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-branches-input',
    styleUrls: ['./branches-input.component.scss'],
    templateUrl: './branches-input.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class BranchesInputComponent {
    /** The form array with one form group per branch condition. */
    public readonly control = input.required<TemplatedFormArray>();

    /** Allows the user to add and remove branches. */
    public readonly isEditable = input.required({ transform: booleanAttribute });

    protected readonly branchesControls = computed<ReadonlyArray<ExtendedFormGroup>>(() => {
        return this.control().controls as any;
    });
}

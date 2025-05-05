/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, ExtendedFormGroup, TemplatedFormArray, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
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
    @Input({ required: true })
    public control!: TemplatedFormArray;

    @Input({ required: true, transform: booleanAttribute })
    public isEditable = true;

    public get branchesControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.control.controls as any;
    }
}
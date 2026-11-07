/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormRowComponent, RolesState, UserInfoFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-user-info-validation',
    styleUrls: ['user-info-validation.component.scss'],
    templateUrl: 'user-info-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class UserInfoValidationComponent {
    protected readonly rolesState = inject(RolesState);

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<UserInfoFieldPropertiesDto>();

    constructor() {
        this.rolesState.loadIfNotLoaded();
    }
}

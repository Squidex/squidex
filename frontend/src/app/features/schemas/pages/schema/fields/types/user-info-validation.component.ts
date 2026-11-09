/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FormRowComponent, RolesState, SignalSelectDirective, UserInfoFieldFormModel, UserInfoFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-user-info-validation',
    styleUrls: ['user-info-validation.component.scss'],
    templateUrl: 'user-info-validation.component.html',
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        SignalSelectDirective,
    ],
})
export class UserInfoValidationComponent {
    protected readonly rolesState = inject(RolesState);

    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<UserInfoFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<UserInfoFieldFormModel>;
    });

    constructor() {
        this.rolesState.loadIfNotLoaded();
    }
}

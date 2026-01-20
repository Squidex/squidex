/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormRowComponent, RolesState, UserInfoFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-user-info-validation',
    styleUrls: ['user-info-validation.component.scss'],
    templateUrl: 'user-info-validation.component.html',
    imports: [
        AsyncPipe,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class UserInfoValidationComponent implements OnInit {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: UserInfoFieldPropertiesDto;

    constructor(
        public readonly rolesState: RolesState,
    ) {
    }

    public ngOnInit() {
        this.rolesState.loadIfNotLoaded();
    }
}

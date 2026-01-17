/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, UserInfoFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-user-info-validation',
    styleUrls: ['user-info-validation.component.scss'],
    templateUrl: 'user-info-validation.component.html',
})
export class UserInfoValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: UserInfoFieldPropertiesDto;
}

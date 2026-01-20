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
    selector: 'sqx-user-info-ui',
    styleUrls: ['user-info-ui.component.scss'],
    templateUrl: 'user-info-ui.component.html',
})
export class UserInfoUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: UserInfoFieldPropertiesDto;
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, UserInfoFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-user-info-ui',
    styleUrls: ['user-info-ui.component.scss'],
    templateUrl: 'user-info-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserInfoUIComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<UserInfoFieldPropertiesDto>();
}

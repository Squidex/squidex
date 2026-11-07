/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-component-ui',
    styleUrls: ['component-ui.component.scss'],
    templateUrl: 'component-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComponentUIComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();
}

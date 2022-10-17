/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-components-ui[field][fieldForm][properties]',
    styleUrls: ['components-ui.component.scss'],
    templateUrl: 'components-ui.component.html',
})
export class ComponentsUIComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: ReferencesFieldPropertiesDto;
}

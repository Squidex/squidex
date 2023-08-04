/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, SchemaTagSource } from '@app/shared';

@Component({
    selector: 'sqx-components-validation',
    styleUrls: ['components-validation.component.scss'],
    templateUrl: 'components-validation.component.html',
})
export class ComponentsValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: ReferencesFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
    }
}

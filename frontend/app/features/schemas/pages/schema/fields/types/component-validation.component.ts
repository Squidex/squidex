/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, SchemaTagSource } from '@app/shared';

@Component({
    selector: 'sqx-component-validation',
    styleUrls: ['component-validation.component.scss'],
    templateUrl: 'component-validation.component.html',
})
export class ComponentValidationComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
    }
}

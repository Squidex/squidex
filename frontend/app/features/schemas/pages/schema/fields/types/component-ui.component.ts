/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-component-ui[field][fieldForm][properties]',
    styleUrls: ['component-ui.component.scss'],
    templateUrl: 'component-ui.component.html',
})
export class ComponentUIComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;
}

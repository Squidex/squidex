/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, REFERENCES_FIELD_EDITORS } from '@app/shared';

@Component({
    selector: 'sqx-references-ui',
    styleUrls: ['references-ui.component.scss'],
    templateUrl: 'references-ui.component.html',
})
export class ReferencesUIComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;

    public editors = REFERENCES_FIELD_EDITORS;
}

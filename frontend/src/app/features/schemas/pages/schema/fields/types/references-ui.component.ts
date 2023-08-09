/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, REFERENCES_FIELD_EDITORS, ReferencesFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-references-ui',
    styleUrls: ['references-ui.component.scss'],
    templateUrl: 'references-ui.component.html',
})
export class ReferencesUIComponent {
    public readonly editors = REFERENCES_FIELD_EDITORS;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: ReferencesFieldPropertiesDto;
}

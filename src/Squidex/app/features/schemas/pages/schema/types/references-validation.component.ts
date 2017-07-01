/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { ReferencesFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesValidationComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: ReferencesFieldPropertiesDto;
}
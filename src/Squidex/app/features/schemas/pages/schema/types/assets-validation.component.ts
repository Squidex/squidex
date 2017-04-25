/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { AssetsFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-assets-validation',
    styleUrls: ['assets-validation.component.scss'],
    templateUrl: 'assets-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsValidationComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: AssetsFieldPropertiesDto;
}
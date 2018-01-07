/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { ReferencesFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-references-ui',
    styleUrls: ['references-ui.component.scss'],
    templateUrl: 'references-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: ReferencesFieldPropertiesDto;
}
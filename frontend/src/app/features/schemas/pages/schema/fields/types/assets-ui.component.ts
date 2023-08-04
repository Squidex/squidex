/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AssetsFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html',
})
export class AssetsUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: AssetsFieldPropertiesDto;
}

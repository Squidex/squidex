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
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: AssetsFieldPropertiesDto;
}
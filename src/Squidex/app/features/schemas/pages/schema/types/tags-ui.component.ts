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
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TagsUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: AssetsFieldPropertiesDto;
}
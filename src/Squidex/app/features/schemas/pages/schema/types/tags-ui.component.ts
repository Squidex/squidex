/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { AssetsFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html'
})
export class TagsUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: AssetsFieldPropertiesDto;
}
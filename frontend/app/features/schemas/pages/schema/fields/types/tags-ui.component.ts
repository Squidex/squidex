/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, TagsFieldPropertiesDto, TAGS_FIELD_EDITORS } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui[field][fieldForm][properties]',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html',
})
export class TagsUIComponent {
    public readonly editors = TAGS_FIELD_EDITORS;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: TagsFieldPropertiesDto;
}

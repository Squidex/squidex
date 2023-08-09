/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, TAGS_FIELD_EDITORS, TagsFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html',
})
export class TagsUIComponent {
    public readonly editors = TAGS_FIELD_EDITORS;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: TagsFieldPropertiesDto;
}

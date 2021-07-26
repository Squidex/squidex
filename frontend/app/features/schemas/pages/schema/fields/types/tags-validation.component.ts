/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, LanguageDto, TagsFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-tags-validation[field][fieldForm][languages][properties]',
    styleUrls: ['tags-validation.component.scss'],
    templateUrl: 'tags-validation.component.html',
})
export class TagsValidationComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: TagsFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;
}

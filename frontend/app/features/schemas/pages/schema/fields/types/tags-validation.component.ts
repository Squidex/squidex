/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { FieldDto, LanguageDto, TagsFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-tags-validation',
    styleUrls: ['tags-validation.component.scss'],
    templateUrl: 'tags-validation.component.html'
})
export class TagsValidationComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: TagsFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable: boolean;

    public ngOnInit() {
        this.fieldForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.fieldForm.setControl('minItems',
            new FormControl(this.properties.minItems));

        this.fieldForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.fieldForm.setControl('defaultValues',
            new FormControl(this.properties.defaultValues));
    }
}
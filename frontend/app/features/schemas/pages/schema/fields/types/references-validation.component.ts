/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { FieldDto, LanguageDto, ReferencesFieldPropertiesDto, SchemaTagSource } from '@app/shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html'
})
export class ReferencesValidationComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    constructor(
        public readonly schemasSource: SchemaTagSource
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('allowDuplicates',
                new FormControl());

            this.fieldForm.setControl('maxItems',
                new FormControl());

            this.fieldForm.setControl('minItems',
                new FormControl());

            this.fieldForm.setControl('schemaIds',
                new FormControl());

            this.fieldForm.setControl('defaultValue',
                new FormControl());

            this.fieldForm.setControl('defaultValues',
                new FormControl());

            this.fieldForm.setControl('mustBePublished',
                new FormControl());
        }

        this.fieldForm.patchValue(this.field.properties);
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { AssetsFieldPropertiesDto, FieldDto, LanguageDto } from '@app/shared';

@Component({
    selector: 'sqx-assets-validation',
    styleUrls: ['assets-validation.component.scss'],
    templateUrl: 'assets-validation.component.html'
})
export class AssetsValidationComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: AssetsFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('minItems',
                new FormControl());

            this.fieldForm.setControl('maxItems',
                new FormControl());

            this.fieldForm.setControl('minSize',
                new FormControl());

            this.fieldForm.setControl('maxSize',
                new FormControl());

            this.fieldForm.setControl('allowedExtensions',
                new FormControl());

            this.fieldForm.setControl('mustBeImage',
                new FormControl());

            this.fieldForm.setControl('minWidth',
                new FormControl());

            this.fieldForm.setControl('maxWidth',
                new FormControl());

            this.fieldForm.setControl('minHeight',
                new FormControl());

            this.fieldForm.setControl('maxHeight',
                new FormControl());

            this.fieldForm.setControl('aspectWidth',
                new FormControl());

            this.fieldForm.setControl('aspectHeight',
                new FormControl());

            this.fieldForm.setControl('allowDuplicates',
                new FormControl());

            this.fieldForm.setControl('defaultValue',
                new FormControl());

            this.fieldForm.setControl('defaultValues',
                new FormControl());
        }

        this.fieldForm.patchValue(this.properties);
    }
}
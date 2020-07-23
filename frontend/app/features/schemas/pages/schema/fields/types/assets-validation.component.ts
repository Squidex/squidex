/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { AssetsFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-assets-validation',
    styleUrls: ['assets-validation.component.scss'],
    templateUrl: 'assets-validation.component.html'
})
export class AssetsValidationComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: AssetsFieldPropertiesDto;

    public ngOnInit() {
        this.fieldForm.setControl('minItems',
            new FormControl(this.properties.minItems));

        this.fieldForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.fieldForm.setControl('minSize',
            new FormControl(this.properties.minSize));

        this.fieldForm.setControl('maxSize',
            new FormControl(this.properties.maxSize));

        this.fieldForm.setControl('allowedExtensions',
            new FormControl(this.properties.allowedExtensions));

        this.fieldForm.setControl('mustBeImage',
            new FormControl(this.properties.mustBeImage));

        this.fieldForm.setControl('minWidth',
            new FormControl(this.properties.minWidth));

        this.fieldForm.setControl('maxWidth',
            new FormControl(this.properties.maxWidth));

        this.fieldForm.setControl('minHeight',
            new FormControl(this.properties.minHeight));

        this.fieldForm.setControl('maxHeight',
            new FormControl(this.properties.maxHeight));

        this.fieldForm.setControl('aspectWidth',
            new FormControl(this.properties.aspectWidth));

        this.fieldForm.setControl('aspectHeight',
            new FormControl(this.properties.aspectHeight));

        this.fieldForm.setControl('allowDuplicates',
            new FormControl(this.properties.allowDuplicates));
    }
}
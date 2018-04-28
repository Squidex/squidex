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
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: AssetsFieldPropertiesDto;

    public ngOnInit() {
        this.editForm.setControl('minItems',
            new FormControl(this.properties.minItems));

        this.editForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.editForm.setControl('minSize',
            new FormControl(this.properties.minSize));

        this.editForm.setControl('maxSize',
            new FormControl(this.properties.maxSize));

        this.editForm.setControl('allowedExtensions',
            new FormControl(this.properties.allowedExtensions));

        this.editForm.setControl('mustBeImage',
            new FormControl(this.properties.mustBeImage));

        this.editForm.setControl('minWidth',
            new FormControl(this.properties.minWidth));

        this.editForm.setControl('maxWidth',
            new FormControl(this.properties.maxWidth));

        this.editForm.setControl('minHeight',
            new FormControl(this.properties.minHeight));

        this.editForm.setControl('maxHeight',
            new FormControl(this.properties.maxHeight));

        this.editForm.setControl('aspectWidth',
            new FormControl(this.properties.aspectWidth));

        this.editForm.setControl('aspectHeight',
            new FormControl(this.properties.aspectHeight));
    }
}
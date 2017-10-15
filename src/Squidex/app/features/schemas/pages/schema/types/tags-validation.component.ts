/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

import { AssetsFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-tags-validation',
    styleUrls: ['tags-validation.component.scss'],
    templateUrl: 'tags-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TagsValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: AssetsFieldPropertiesDto;

    public ngOnInit() {
        this.editForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.editForm.setControl('minItems',
            new FormControl(this.properties.minItems));
    }
}
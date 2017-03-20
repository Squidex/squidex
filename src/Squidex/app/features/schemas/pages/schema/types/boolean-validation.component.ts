/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import { BooleanFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-boolean-validation',
    styleUrls: ['boolean-validation.component.scss'],
    templateUrl: 'boolean-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class BooleanValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    public hideDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.hideDefaultValue =
            this.editForm.get('isRequired').valueChanges
                .startWith(this.properties.isRequired)
                .map(x => !!x);
    }
}
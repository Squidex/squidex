/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html'
})
export class NumberValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    public hideDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('maxValue',
            new FormControl());
        this.editForm.addControl('minValue',
            new FormControl());
        this.editForm.addControl('pattern',
            new FormControl());
        this.editForm.addControl('patternMessage',
            new FormControl());
        this.editForm.addControl('defaultValue',
            new FormControl());

        this.hideDefaultValue =
            Observable.of(false)
                .merge(this.editForm.get('isRequired').valueChanges)
                .map(x => !!x);
    }
}
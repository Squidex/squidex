/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { FloatConverter, NumberFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-number-ui',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html'
})
export class NumberUIComponent implements OnDestroy, OnInit {
    private hideAllowedValuesSubscription: Subscription;
    private hideInlineEditableSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public converter = new FloatConverter();

    public hideAllowedValues: Observable<boolean>;
    public hideInlineEditable: Observable<boolean>;

    public ngOnDestroy() {
        this.hideAllowedValuesSubscription.unsubscribe();
        this.hideInlineEditableSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('placeholder',
            new FormControl(this.properties.placeholder, [
                Validators.maxLength(100)
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues, []));

        this.editForm.setControl('inlineEditable',
            new FormControl(this.properties.inlineEditable));

        this.hideAllowedValues =
            this.editForm.controls['editor'].valueChanges
                .startWith(this.properties.editor)
                .map(x => !(x && (x === 'Radio' || x === 'Dropdown')));

        this.hideInlineEditable =
            this.editForm.controls['editor'].valueChanges
                .startWith(this.properties.editor)
                .map(x => !(x && (x === 'Input' || x === 'Dropdown')));

        this.hideAllowedValuesSubscription =
            this.hideAllowedValues.subscribe(isSelection => {
                if (isSelection) {
                    this.editForm.controls['allowedValues'].setValue(undefined);
                }
            });

        this.hideInlineEditableSubscription =
            this.hideInlineEditable.subscribe(isSelection => {
                if (isSelection) {
                    this.editForm.controls['inlineEditable'].setValue(false);
                }
            });
    }
}
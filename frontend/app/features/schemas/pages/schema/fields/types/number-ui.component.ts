/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FieldDto, FloatConverter, NumberFieldPropertiesDto, NUMBER_FIELD_EDITORS, ResourceOwner, value$ } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-number-ui',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html'
})
export class NumberUIComponent extends ResourceOwner implements OnInit {
    public readonly converter = FloatConverter.INSTANCE;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public editors = NUMBER_FIELD_EDITORS;

    public hideAllowedValues: Observable<boolean>;
    public hideInlineEditable: Observable<boolean>;

    public ngOnInit() {
        this.fieldForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.fieldForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues, []));

        this.fieldForm.setControl('inlineEditable',
            new FormControl(this.properties.inlineEditable));

        this.hideAllowedValues =
            value$<string>(this.fieldForm.controls['editor']).pipe(map(x => !(x && (x === 'Radio' || x === 'Dropdown'))));

        this.hideInlineEditable =
            value$<string>(this.fieldForm.controls['editor']).pipe(map(x => x === 'Radio'));

        this.own(
            this.hideAllowedValues.subscribe(isSelection => {
                if (isSelection) {
                    this.fieldForm.controls['allowedValues'].setValue(undefined);
                }
            }));

        this.own(
            this.hideInlineEditable.subscribe(isSelection => {
                if (isSelection) {
                    this.fieldForm.controls['inlineEditable'].setValue(false);
                }
            }));
    }
}
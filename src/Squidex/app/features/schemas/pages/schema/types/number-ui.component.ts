/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
    FieldDto,
    FloatConverter,
    NumberFieldPropertiesDto,
    ResourceOwner,
    value$
} from '@app/shared';

@Component({
    selector: 'sqx-number-ui',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html'
})
export class NumberUIComponent extends ResourceOwner implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public converter = new FloatConverter();

    public hideAllowedValues: Observable<boolean>;
    public hideInlineEditable: Observable<boolean>;

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues, []));

        this.editForm.setControl('inlineEditable',
            new FormControl(this.properties.inlineEditable));

        this.hideAllowedValues =
            value$<string>(this.editForm.controls['editor']).pipe(map(x => !(x && (x === 'Radio' || x === 'Dropdown'))));

        this.hideInlineEditable =
            value$<string>(this.editForm.controls['editor']).pipe(map(x => x === 'Radio'));

        this.own(
            this.hideAllowedValues.subscribe(isSelection => {
                if (isSelection) {
                    this.editForm.controls['allowedValues'].setValue(undefined);
                }
            }));

        this.own(
            this.hideInlineEditable.subscribe(isSelection => {
                if (isSelection) {
                    this.editForm.controls['inlineEditable'].setValue(false);
                }
            }));
    }
}
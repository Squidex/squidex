/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, FloatConverter, NumberFieldPropertiesDto, NUMBER_FIELD_EDITORS, ResourceOwner, value$ } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-number-ui[field][fieldForm][properties]',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html',
})
export class NumberUIComponent extends ResourceOwner implements OnChanges {
    public readonly converter = FloatConverter.INSTANCE;
    public readonly editors = NUMBER_FIELD_EDITORS;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public hideAllowedValues: Observable<boolean>;
    public hideInlineEditable: Observable<boolean>;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.unsubscribeAll();

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
}

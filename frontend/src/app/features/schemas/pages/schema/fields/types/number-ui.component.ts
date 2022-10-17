/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { FieldDto, FloatConverter, NUMBER_FIELD_EDITORS, NumberFieldPropertiesDto, ResourceOwner, valueProjection$ } from '@app/shared';

@Component({
    selector: 'sqx-number-ui[field][fieldForm][properties]',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html',
})
export class NumberUIComponent extends ResourceOwner implements OnChanges {
    public readonly converter = FloatConverter.INSTANCE;
    public readonly editors = NUMBER_FIELD_EDITORS;

    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: NumberFieldPropertiesDto;

    public hideAllowedValues?: Observable<boolean>;
    public hideInlineEditable?: Observable<boolean>;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.unsubscribeAll();

            const editor = this.fieldForm.controls['editor'];

            this.hideAllowedValues =
                valueProjection$(editor, x => !(x && (x === 'Radio' || x === 'Dropdown')));

            this.hideInlineEditable =
                valueProjection$(editor, x => x === 'Radio');

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

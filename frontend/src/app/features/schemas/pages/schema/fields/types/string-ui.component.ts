/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { FieldDto, ResourceOwner, SchemaTagSource, STRING_FIELD_EDITORS, StringFieldPropertiesDto, valueProjection$ } from '@app/shared';

@Component({
    selector: 'sqx-string-ui[field][fieldForm][properties]',
    styleUrls: ['string-ui.component.scss'],
    templateUrl: 'string-ui.component.html',
})
export class StringUIComponent extends ResourceOwner implements OnChanges {
    public readonly editors = STRING_FIELD_EDITORS;

    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: StringFieldPropertiesDto;

    public hideAllowedValues?: Observable<boolean>;
    public hideInlineEditable?: Observable<boolean>;
    public hideSchemaIds?: Observable<boolean>;

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
        super();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.unsubscribeAll();

            const editor = this.fieldForm.controls['editor'];

            this.hideAllowedValues =
                valueProjection$(editor, x => !(x && (x === 'Radio' || x === 'Dropdown')));

            this.hideInlineEditable =
                valueProjection$(editor, x => !(x && (x === 'Input' || x === 'Dropdown' || x === 'Slug')));

            this.hideSchemaIds =
                valueProjection$(this.fieldForm.controls['isEmbeddable'], x => !x);

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

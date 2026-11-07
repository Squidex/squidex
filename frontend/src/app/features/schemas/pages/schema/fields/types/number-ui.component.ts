/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, input, untracked } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { switchMap } from 'rxjs/operators';
import { FieldDto, FloatConverter, FormRowComponent, NumberFieldEditorValues, NumberFieldPropertiesDto, TagEditorComponent, valueProjection$ } from '@app/shared';

@Component({
    selector: 'sqx-number-ui',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
    ],
})
export class NumberUIComponent {
    protected readonly converter = FloatConverter.INSTANCE;
    protected readonly editors = NumberFieldEditorValues;

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<NumberFieldPropertiesDto>();

    protected readonly hideAllowedValues = toSignal(
        toObservable(this.fieldForm).pipe(
            switchMap(form => valueProjection$(form.controls['editor'], hidesAllowedValues))),
        { initialValue: false });

    constructor() {
        effect(onCleanup => {
            const fieldForm = this.fieldForm();

            const subscription = untracked(() =>
                valueProjection$(fieldForm.controls['editor'], hidesAllowedValues)
                    .subscribe(isHidden => {
                        if (isHidden) {
                            fieldForm.controls['allowedValues'].setValue(undefined);
                        }
                    }));

            onCleanup(() => subscription.unsubscribe());
        });

        effect(onCleanup => {
            const fieldForm = this.fieldForm();

            const subscription = untracked(() =>
                valueProjection$(fieldForm.controls['editor'], hidesInlineEditable)
                    .subscribe(isHidden => {
                        if (isHidden) {
                            fieldForm.controls['inlineEditable'].setValue(false);
                        }
                    }));

            onCleanup(() => subscription.unsubscribe());
        });
    }
}

function hidesAllowedValues(editor: string) {
    return !(editor && (editor === 'Radio' || editor === 'Dropdown'));
}

function hidesInlineEditable(editor: string) {
    return editor === 'Radio';
}

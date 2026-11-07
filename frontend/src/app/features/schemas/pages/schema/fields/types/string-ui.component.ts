/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, input, untracked, inject } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { switchMap } from 'rxjs/operators';
import { AssetFolderDropdownComponent, FieldDto, FormRowComponent, SchemaTagSource, StringFieldEditorValues, StringFieldPropertiesDto, TagEditorComponent, TranslatePipe, valueProjection$ } from '@app/shared';

@Component({
    selector: 'sqx-string-ui',
    styleUrls: ['string-ui.component.scss'],
    templateUrl: 'string-ui.component.html',
    imports: [
        AssetFolderDropdownComponent,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class StringUIComponent {
    protected readonly schemasSource = inject(SchemaTagSource);

    protected readonly editors = StringFieldEditorValues;

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<StringFieldPropertiesDto>();

    protected readonly hideAllowedValues = toSignal(
        toObservable(this.fieldForm).pipe(
            switchMap(form => valueProjection$(form.controls['editor'], hidesAllowedValues))),
        { initialValue: false });

    protected readonly hideClassNames = toSignal(
        toObservable(this.fieldForm).pipe(
            switchMap(form => valueProjection$(form.controls['editor'], hidesClassNames))),
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

function hidesClassNames(editor: string) {
    return !(editor && editor === 'RichText');
}

function hidesInlineEditable(editor: string) {
    return !(editor && (editor === 'Input' || editor === 'Dropdown' || editor === 'Slug'));
}

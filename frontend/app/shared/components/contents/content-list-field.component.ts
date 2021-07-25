/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ContentDto, FieldValue, getContentValue, LanguageDto, MetaFields, RootFieldDto, StatefulComponent, TableField, Types } from '@app/shared/internal';

interface State {
    // The formatted value.
    formatted: FieldValue;
}

@Component({
    selector: 'sqx-content-list-field[content][field][language]',
    styleUrls: ['./content-list-field.component.scss'],
    templateUrl: './content-list-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentListFieldComponent extends StatefulComponent<State> implements OnChanges {
    @Input()
    public field: TableField;

    @Input()
    public content: ContentDto;

    @Input()
    public patchAllowed?: boolean | null;

    @Input()
    public patchForm?: FormGroup | null;

    @Input()
    public language: LanguageDto;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            formatted: '',
        });
    }

    public ngOnChanges() {
        this.reset();
    }

    public reset() {
        if (Types.is(this.field, RootFieldDto)) {
            const { value, formatted } = getContentValue(this.content, this.language, this.field);

            if (this.patchForm) {
                const formControl = this.patchForm.controls[this.field.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }

            this.next({ formatted });
        }
    }

    public get metaFields() {
        return MetaFields;
    }

    public get isInlineEditable() {
        return Types.is(this.field, RootFieldDto) ? this.field.isInlineEditable : false;
    }

    public get fieldName() {
        return Types.is(this.field, RootFieldDto) ? this.field.name : this.field;
    }
}

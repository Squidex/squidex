/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, QueryList, ViewChildren } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldArrayItemForm, FieldSection, NestedFieldDto } from '@app/shared';
import { FieldEditorComponent } from './field-editor.component';

@Component({
    selector: 'sqx-array-section',
    styleUrls: ['./array-section.component.scss'],
    templateUrl: './array-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArraySectionComponent {
    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formSection: FieldSection<NestedFieldDto, FieldArrayItemForm>;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public canUnset: boolean;

    @ViewChildren(FieldEditorComponent)
    public editors: QueryList<FieldEditorComponent>;

    public reset() {
        this.editors.forEach(editor => {
            editor.reset();
        });
    }

    public trackByField(_index: number, field: FieldArrayItemForm) {
        return field.field.fieldId;
    }
}
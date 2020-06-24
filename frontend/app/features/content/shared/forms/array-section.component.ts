/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, QueryList, ViewChildren } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppLanguageDto, EditContentForm, NestedFieldDto } from '@app/shared';
import { FieldSection } from './../group-fields.pipe';
import { FieldEditorComponent } from './field-editor.component';

@Component({
    selector: 'sqx-array-section',
    styleUrls: ['./array-section.component.scss'],
    templateUrl: './array-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArraySectionComponent {
    @Input()
    public itemForm: FormGroup;

    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public section: FieldSection<NestedFieldDto>;

    @ViewChildren(FieldEditorComponent)
    public editors: QueryList<FieldEditorComponent>;

    public getControl(field: NestedFieldDto) {
        return this.itemForm.get(field.name)!;
    }

    public reset() {
        this.editors.forEach(editor => {
            editor.reset();
        });
    }

    public trackByField(index: number, field: NestedFieldDto) {
        return field.fieldId;
    }
}
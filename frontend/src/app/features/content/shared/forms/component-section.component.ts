/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, QueryList, ViewChildren } from '@angular/core';
import { AbstractContentForm, AppLanguageDto, EditContentForm, FieldDto, FieldSection } from '@app/shared';
import { FieldEditorComponent } from './field-editor.component';

@Component({
    selector: 'sqx-component-section',
    styleUrls: ['./component-section.component.scss'],
    templateUrl: './component-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComponentSectionComponent {
    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true })
    public formLevel!: number;

    @Input({ required: true })
    public formSection!: FieldSection<FieldDto, any>;

    @Input({ required: true })
    public isComparing = false;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public index: number | null | undefined;

    @Input()
    public canUnset?: boolean | null;

    @ViewChildren(FieldEditorComponent)
    public editors!: QueryList<FieldEditorComponent>;

    public reset() {
        this.editors.forEach(editor => {
            editor.reset();
        });
    }

    public trackByField(_index: number, field: AbstractContentForm<any, any>) {
        return field.field.fieldId;
    }
}

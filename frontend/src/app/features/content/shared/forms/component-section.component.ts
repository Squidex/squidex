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
    selector: 'sqx-component-section[form][formContext][formLevel][formSection][isComparing][language][languages]',
    styleUrls: ['./component-section.component.scss'],
    templateUrl: './component-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComponentSectionComponent {
    @Input()
    public form!: EditContentForm;

    @Input()
    public formContext!: any;

    @Input()
    public formLevel!: number;

    @Input()
    public formSection!: FieldSection<FieldDto, any>;

    @Input()
    public isComparing = false;

    @Input()
    public language!: AppLanguageDto;

    @Input()
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

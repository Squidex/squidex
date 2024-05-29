/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input, numberAttribute, QueryList, ViewChildren } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldDto, FieldSection, FormHintComponent, MarkdownDirective } from '@app/shared';
import { FieldEditorComponent } from './field-editor.component';

@Component({
    standalone: true,
    selector: 'sqx-component-section',
    styleUrls: ['./component-section.component.scss'],
    templateUrl: './component-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        FormHintComponent,
        MarkdownDirective,
        forwardRef(() => FieldEditorComponent),
    ],
})
export class ComponentSectionComponent {
    @Input({ required: true })
    public hasChatBot!: boolean;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formSection!: FieldSection<FieldDto, any>;

    @Input({ required: true, transform: booleanAttribute })
    public isComparing = false;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ transform: numberAttribute })
    public index: number | null | undefined;

    @ViewChildren(FieldEditorComponent)
    public editors!: QueryList<FieldEditorComponent>;

    public reset() {
        this.editors.forEach(editor => {
            editor.reset();
        });
    }
}

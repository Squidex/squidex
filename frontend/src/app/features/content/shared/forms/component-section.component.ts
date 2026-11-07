/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, input, numberAttribute, viewChildren } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldDto, FieldSection, FormHintComponent, MarkdownDirective } from '@app/shared';
import { FieldEditorComponent } from './field-editor.component';

@Component({
    selector: 'sqx-component-section',
    styleUrls: ['./component-section.component.scss'],
    templateUrl: './component-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormHintComponent,
        MarkdownDirective,
        forwardRef(() => FieldEditorComponent),
    ],
})
export class ComponentSectionComponent {
    private readonly editors = viewChildren(FieldEditorComponent);

    /** Enables the AI features of the editors. */
    public readonly hasChatBot = input.required<boolean>();

    /** The content form the section belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The nesting level of the section. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** The section with the separator and the fields to render. */
    public readonly formSection = input.required<FieldSection<FieldDto, any>>();

    /** Renders the editors next to the editors of another version. */
    public readonly isComparing = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** The language of the field values. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The index of the array item the section belongs to. */
    public readonly index = input<number | null | undefined, unknown>(undefined, { transform: numberAttribute });

    public reset() {
        for (const editor of this.editors()) {
            editor.reset();
        }
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, inject, input, linkedSignal, numberAttribute, output } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldDto, FieldForm, FieldSection, FormHintComponent, LocalStoreService, MarkdownDirective, SchemaDto, Settings } from '@app/shared';
import { ContentFieldComponent } from './content-field.component';

@Component({
    selector: 'sqx-content-section',
    styleUrls: ['./content-section.component.scss'],
    templateUrl: './content-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        forwardRef(() => ContentFieldComponent),
        FormHintComponent,
        MarkdownDirective,
    ],
})
export class ContentSectionComponent {
    private readonly localStore = inject(LocalStoreService);

    /** Emits when the user selects another language. */
    public readonly languageChange = output<AppLanguageDto>();

    /** Renders the fields with full width, even if they are configured as half width fields. */
    public readonly isCompact = input(false, { transform: booleanAttribute });

    /** The content form the section belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** The content form of the other version, when comparing two versions. */
    public readonly formCompare = input<EditContentForm | null>();

    /** The nesting level of the section, 0 for root sections. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The section with the separator and the fields to render. */
    public readonly formSection = input.required<FieldSection<FieldDto, FieldForm>>();

    /** The schema of the content. */
    public readonly schema = input.required<SchemaDto>();

    /** The selected language. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly isCollapsed = linkedSignal(() => {
        if (!this.formSection().separator) {
            return false;
        }

        return this.localStore.getBoolean(this.isCollapsedKey());
    });

    protected toggle() {
        this.isCollapsed.update(x => !x);

        if (this.formSection().separator) {
            this.localStore.setBoolean(this.isCollapsedKey(), this.isCollapsed());
        }
    }

    protected getFieldFormCompare(formState: FieldForm) {
        return this.formCompare()?.get(formState.field.name);
    }

    private isCollapsedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.schema()?.id, this.formSection()?.separator?.fieldId);
    }
}

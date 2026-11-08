/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, input, model } from '@angular/core';
import { map } from 'rxjs';
import { AppLanguageDto, AppsState, CheckboxGroupComponent, ContentDto, ContentsService, FormValueControlBase, LocalizerService, safeResource, UIOptions } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

@Component({
    selector: 'sqx-references-checkboxes',
    styleUrls: ['./references-checkboxes.component.scss'],
    templateUrl: './references-checkboxes.component.html',
    imports: [
        CheckboxGroupComponent,
    ],
})
export class ReferencesCheckboxesComponent extends FormValueControlBase<ReadonlyArray<string> | null> {
    private readonly appsState = inject(AppsState);
    private readonly contentsService = inject(ContentsService);
    private readonly localizer = inject(LocalizerService);
    private readonly uiOptions = inject(UIOptions);
    private readonly itemCount: number = this.uiOptions.value.referencesDropdownItemCount;

    /** The value of the control. */
    public readonly value = model<ReadonlyArray<string> | null>(null);

    /** The ID of the schema to select the contents from. */
    public readonly schemaId = input.required<string | undefined | null>();

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

    protected readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    protected readonly isSuccess = computed(() => {
        const contentItems = this.contentItems.value();

        return this.isValid() && !!contentItems && contentItems.length > 0;
    });

    protected readonly isGroupDisabled = computed(() => {
        return this.disabled() || !this.isSuccess();
    });

    protected readonly converter = computed(() => {
        const contentItems = this.contentItems.value();
        if (!this.isSuccess() || !contentItems) {
            return new ReferencesTagsConverter(null!, [], this.localizer);
        }

        return new ReferencesTagsConverter(this.language(), contentItems, this.localizer);
    });

    protected readonly contentItems = safeResource({
        params: () => this.schemaId() || undefined,
        stream: ({ params: schemaId }) => this.contentsService.getContents(this.appsState.appName, schemaId, { take: this.itemCount }).pipe(
            map(contents => contents.items as ReadonlyArray<ContentDto> | null)),
        defaultValue: null,
    });

    protected updateValue(value: ReadonlyArray<string> | null | undefined) {
        this.markTouched();

        if (value && value.length > 0) {
            this.value.set(value);
        } else {
            this.value.set(null);
        }
    }
}

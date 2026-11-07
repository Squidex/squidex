/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, inject, input, model } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormValueControl } from '@angular/forms/signals';
import { catchError, map, of } from 'rxjs';
import { AppLanguageDto, AppsState, CheckboxGroupComponent, ContentDto, ContentsService, LocalizerService, UIOptions } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

@Component({
    selector: 'sqx-references-checkboxes',
    styleUrls: ['./references-checkboxes.component.scss'],
    templateUrl: './references-checkboxes.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CheckboxGroupComponent,
    ],
})
export class ReferencesCheckboxesComponent implements FormValueControl<ReadonlyArray<string> | null> {
    private readonly appsState = inject(AppsState);
    private readonly contentsService = inject(ContentsService);
    private readonly localizer = inject(LocalizerService);
    private readonly uiOptions = inject(UIOptions);
    private readonly itemCount: number = this.uiOptions.value.referencesDropdownItemCount;
    private readonly contentItems = rxResource({
        params: () => this.schemaId() || undefined,
        stream: ({ params: schemaId }) => this.contentsService.getContents(this.appsState.appName, schemaId, { take: this.itemCount }).pipe(
            map(contents => contents.items as ReadonlyArray<ContentDto> | null),
            catchError(() => of(null))),
        defaultValue: null,
    });

    private readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    private readonly isSuccess = computed(() => {
        const contentItems = this.contentItems.value();

        return this.isValid() && !!contentItems && contentItems.length > 0;
    });

    /** The value of the control. */
    public readonly value = model<ReadonlyArray<string> | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The ID of the schema to select the contents from. */
    public readonly schemaId = input.required<string | undefined | null>();

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

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

    protected updateValue(value: ReadonlyArray<string> | null | undefined) {
        this.touched.set(true);

        if (value && value.length > 0) {
            this.value.set(value);
        } else {
            this.value.set(null);
        }
    }
}

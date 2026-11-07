/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, inject, input, model, signal } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { AppLanguageDto, AppsState, ContentDto, ContentsService, LocalizerService, RadioGroupComponent, UIOptions } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

@Component({
    selector: 'sqx-references-radio-buttons',
    styleUrls: ['./references-radio-buttons.component.scss'],
    templateUrl: './references-radio-buttons.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RadioGroupComponent,
    ],
})
export class ReferencesRadioButtonsComponent implements FormValueControl<ReadonlyArray<string> | null> {
    private readonly appsState = inject(AppsState);
    private readonly contentsService = inject(ContentsService);
    private readonly localizer = inject(LocalizerService);
    private readonly itemCount: number = inject(UIOptions).value.referencesDropdownItemCount;
    private readonly contentItems = signal<ReadonlyArray<ContentDto> | null>(null);

    private readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    private readonly isSuccess = computed(() => {
        const contentItems = this.contentItems();

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

    protected readonly selectedId = computed(() => {
        return this.value()?.[0] ?? null;
    });

    protected readonly converter = computed(() => {
        const contentItems = this.contentItems();
        if (!this.isSuccess() || !contentItems) {
            return new ReferencesTagsConverter(null!, [], this.localizer);
        }

        return new ReferencesTagsConverter(this.language(), contentItems, this.localizer);
    });

    constructor() {
        effect(onCleanup => {
            const schemaId = this.schemaId();
            if (!schemaId) {
                this.contentItems.set(null);
                return;
            }

            const subscription =
                this.contentsService.getContents(this.appsState.appName, schemaId, { take: this.itemCount })
                    .subscribe({
                        next: contents => {
                            this.contentItems.set(contents.items);
                        },
                        error: () => {
                            this.contentItems.set(null);
                        },
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected updateValue(value: string | null | undefined) {
        this.touched.set(true);

        if (value) {
            this.value.set([value]);
        } else {
            this.value.set(null);
        }
    }
}

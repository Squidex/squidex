/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, input, linkedSignal, model, signal, untracked } from '@angular/core';
import { Observable } from 'rxjs';
import { AppLanguageDto, ContentDto, ContentsDto, FormValueControlBase, LocalizerService, ResolveContents, TagEditorComponent, TranslatePipe, Types } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

@Component({
    selector: 'sqx-references-tags',
    styleUrls: ['./references-tags.component.scss'],
    templateUrl: './references-tags.component.html',
    imports: [
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ReferencesTagsComponent extends FormValueControlBase<ReadonlyArray<string> | null> {
    private readonly contentsResolver = inject(ResolveContents);
    private readonly localizer = inject(LocalizerService);
    private isOpenedBefore = false;

    /** The value of the control. */
    public readonly value = model<ReadonlyArray<string> | null>(null);

    /** The ID of the schema to select the contents from. */
    public readonly schemaId = input.required<string>();

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly contents = linkedSignal<string, ReadonlyArray<ContentDto>>({
        source: () => this.schemaId(),
        computation: () => [],
    });

    protected readonly contentsFailed = signal(false);

    protected readonly contentsLoading = signal(false);

    protected readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    protected readonly selectedIds = computed(() => {
        const value = this.value();

        return Types.isArrayOfString(value) ? value : null;
    });

    protected readonly isEditorDisabled = computed(() => {
        return this.disabled() || !this.isValid() || this.contentsFailed();
    });

    protected readonly converter = computed(() => {
        return new ReferencesTagsConverter(this.language(), this.contents(), this.localizer);
    });

    constructor() {
        super();

        effect(() => {
            const ids = this.selectedIds();

            untracked(() => {
                const contents = this.contents();

                const idsNew = ids?.filter(x => !contents.find(y => y.id === x));
                if (idsNew && idsNew.length > 0) {
                    this.loadMore(this.contentsResolver.resolveMany(idsNew));
                }
            });
        });
    }

    protected onOpened() {
        if (this.isOpenedBefore) {
            return;
        }

        this.isOpenedBefore = true;
        this.loadMore(this.contentsResolver.resolveAll(this.schemaId()));
    }

    protected updateValue(value: ReadonlyArray<string> | null | undefined) {
        this.markTouched();

        if (value && value.length > 0) {
            this.value.set(value);
        } else {
            this.value.set(null);
        }
    }

    private loadMore(observable: Observable<ContentsDto>) {
        this.contentsLoading.set(true);

        observable
            .subscribe({
                next: ({ items }) => {
                    this.contentsLoading.set(false);

                    if (items.length === 0) {
                        return;
                    }

                    this.contents.update(contents => {
                        const result = [...contents];

                        for (const content of items) {
                            const index = result.findIndex(x => x.id === content.id);

                            if (index >= 0) {
                                result[index] = content;
                            } else {
                                result.push(content);
                            }
                        }

                        return result;
                    });

                    this.contentsFailed.set(false);
                },
                error: () => {
                    this.contentsLoading.set(false);
                    this.contentsFailed.set(true);
                },
            });
    }
}

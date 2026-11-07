/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, model, signal, untracked } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { Observable } from 'rxjs';
import { AppLanguageDto, ContentDto, ContentsDto, DropdownComponent, getContentValue, HighlightPipe, LocalizerService, ResolveContents, SafeHtmlPipe, Types } from '@app/shared';

type ContentName = { name: string; id?: string };

@Component({
    selector: 'sqx-reference-dropdown',
    styleUrls: ['./reference-dropdown.component.scss'],
    templateUrl: './reference-dropdown.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownComponent,
        HighlightPipe,
        SafeHtmlPipe,
    ],
})
export class ReferenceDropdownComponent implements FormValueControl<ReadonlyArray<string> | string | null> {
    private readonly contentsResolver = inject(ResolveContents);
    private readonly localizer = inject(LocalizerService);
    private isOpenedBefore = false;

    /** The value of the control. */
    public readonly value = model<ReadonlyArray<string> | string | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The ID of the schema to select the content from. */
    public readonly schemaId = input.required<string>();

    /** Defines whether the value is a single ID or an array with one ID. */
    public readonly mode = input.required<'Array' | 'Single'>();

    protected readonly contents = linkedSignal<string, ReadonlyArray<ContentDto>>({
        source: () => this.schemaId(),
        computation: () => [],
    });

    protected readonly contentsFailed = signal(false);

    protected readonly contentsLoading = signal(false);

    protected readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    protected readonly selectedId = computed(() => {
        const value = this.value();

        if (Types.isString(value)) {
            return value;
        } else if (Types.isArrayOfString(value)) {
            return value[0] ?? null;
        } else {
            return null;
        }
    });

    protected readonly isDropdownDisabled = computed(() => {
        return this.disabled() || !this.isValid() || this.contentsFailed();
    });

    protected readonly contentNames = computed(() => {
        const language = this.language();

        const contentNames: ContentName[] = [
            { name: this.localizer.getOrKey('contents.noReference') },
        ];

        for (const content of this.contents()) {
            const name =
                content.referenceFields
                    .map(f => getContentValue(content, language, f, false))
                    .map(v => v.formatted)
                    .defined()
                    .join(', ')
                || this.localizer.getOrKey('common.noValue');

            contentNames.push({ name, id: content.id });
        }

        return contentNames;
    });

    constructor() {
        effect(() => {
            const id = this.selectedId();

            untracked(() => {
                if (!id) {
                    return;
                }

                const isNewId = !this.contents().find(x => x.id === id);
                if (isNewId) {
                    this.loadMore(this.contentsResolver.resolveMany([id]));
                }
            });
        });
    }

    protected updateValue(id: string | null | undefined) {
        if (this.isDropdownDisabled()) {
            return;
        }

        const isSingle = this.mode() === 'Single';

        if (id) {
            this.value.set(isSingle ? id : [id]);
        } else {
            this.value.set(isSingle ? null : []);
        }

        this.touched.set(true);
    }

    protected onOpened() {
        if (this.isOpenedBefore) {
            return;
        }

        this.isOpenedBefore = true;
        this.loadMore(this.contentsResolver.resolveAll(this.schemaId()));
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

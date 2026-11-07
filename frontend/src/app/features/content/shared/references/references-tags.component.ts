/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, forwardRef, input, linkedSignal, signal } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { AppLanguageDto, ContentDto, ContentsDto, LocalizerService, ResolveContents, SignalControlComponent, TagEditorComponent, TranslatePipe, Types } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

export const SQX_REFERENCES_TAGS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesTagsComponent), multi: true,
};

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-references-tags',
    styleUrls: ['./references-tags.component.scss'],
    templateUrl: './references-tags.component.html',
    providers: [
        SQX_REFERENCES_TAGS_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ReferencesTagsComponent extends SignalControlComponent<ReadonlyArray<string>> {
    private isOpenedBefore = false;

    /** The ID of the schema to select the contents from. */
    public readonly schemaId = input.required<string>();

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly control = new UntypedFormControl([]);

    private readonly contents = linkedSignal<string, ReadonlyArray<ContentDto>>({
        source: this.schemaId,
        computation: () => [],
    });

    private readonly contentsFailed = signal(false);
    protected readonly contentsLoading = signal(false);

    protected readonly converter = computed(() => {
        return new ReferencesTagsConverter(this.language(), this.contents(), this.localizer);
    });

    private readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    constructor(
        private readonly contentsResolver: ResolveContents,
        private readonly localizer: LocalizerService,
    ) {
        super();

        effect(() => {
            if (this.isDisabled() || !this.isValid() || this.contentsFailed()) {
                this.control.disable(NO_EMIT);
            } else {
                this.control.enable(NO_EMIT);
            }
        });

        effect(onCleanup => {
            const subscription =
                this.control.valueChanges
                    .subscribe((value: string[]) => {
                        this.callTouched();

                        if (value && value.length > 0) {
                            this.callChange(value);
                        } else {
                            this.callChange(null);
                        }
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected onOpened() {
        if (this.isOpenedBefore) {
            return;
        }

        this.isOpenedBefore = true;
        this.loadMore(this.contentsResolver.resolveAll(this.schemaId()));
    }

    public writeValue(obj: ReadonlyArray<string>) {
        if (Types.isArrayOfString(obj)) {
            this.selectContent(obj);
        } else {
            this.selectContent(undefined);
        }
    }

    private selectContent(ids?: ReadonlyArray<string>) {
        const contents = this.contents();

        const idsNew = ids?.filter(x => !contents.find(y => y.id === x));
        if (idsNew && idsNew.length > 0) {
            this.loadMore(this.contentsResolver.resolveMany(idsNew));
        }

        this.control.setValue(ids, NO_EMIT);
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

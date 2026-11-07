/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, forwardRef, inject, input, linkedSignal, signal } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { AppLanguageDto, ContentDto, ContentsDto, DropdownComponent, getContentValue, HighlightPipe, LocalizerService, ResolveContents, SafeHtmlPipe, SignalControlComponent, Types } from '@app/shared';

export const SQX_REFERENCE_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferenceDropdownComponent), multi: true,
};

type ContentName = { name: string; id?: string };

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-reference-dropdown',
    styleUrls: ['./reference-dropdown.component.scss'],
    templateUrl: './reference-dropdown.component.html',
    providers: [
        SQX_REFERENCE_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownComponent,
        FormsModule,
        HighlightPipe,
        ReactiveFormsModule,
        SafeHtmlPipe,
    ],
})
export class ReferenceDropdownComponent extends SignalControlComponent<ReadonlyArray<string> | string> {
    private readonly contentsResolver = inject(ResolveContents);
    private readonly localizer = inject(LocalizerService);

    private readonly contents = linkedSignal<string, ReadonlyArray<ContentDto>>({
        source: () => this.schemaId(),
        computation: () => [],
    });

    private readonly contentsFailed = signal(false);

    private readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    private isOpenedBefore = false;

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The ID of the schema to select the content from. */
    public readonly schemaId = input.required<string>();

    /** Defines whether the value is a single ID or an array with one ID. */
    public readonly mode = input.required<'Array' | 'Single'>();

    protected readonly control = new UntypedFormControl('');

    protected readonly contentsLoading = signal(false);

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
                    .subscribe((id: string) => {
                        if (!this.control.enabled) {
                            return;
                        }

                        const isSingle = this.mode() === 'Single';

                        if (id) {
                            this.callChange(isSingle ? id : [id]);
                        } else {
                            this.callChange(isSingle ? null : []);
                        }

                        this.callTouched();
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public writeValue(obj: any) {
        if (Types.isString(obj)) {
            this.selectContent(obj);
        } else if (Types.isArrayOfString(obj)) {
            this.selectContent(obj[0]);
        } else {
            this.selectContent(undefined);
        }
    }

    protected onOpened() {
        if (this.isOpenedBefore) {
            return;
        }

        this.isOpenedBefore = true;
        this.loadMore(this.contentsResolver.resolveAll(this.schemaId()));
    }

    private selectContent(id: string | undefined) {
        const isNewId = !this.contents().find(x => x.id === id);

        if (id && isNewId) {
            this.loadMore(this.contentsResolver.resolveMany([id]));
        }

        this.control.setValue(id, NO_EMIT);
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

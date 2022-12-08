/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NG_VALUE_ACCESSOR, UntypedFormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { ContentsDto } from '@app/shared';
import { ContentDto, getContentValue, LanguageDto, LocalizerService, ResolveContents, StatefulControlComponent, Types, value$ } from '@app/shared/internal';

export const SQX_REFERENCE_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferenceDropdownComponent), multi: true,
};

interface State {
    // The names of the selected content items for search.
    contentNames: ReadonlyArray<ContentName>;

    // The name of the selected item.
    selectedItem?: ContentName;

    // True when loading.
    isLoading?: boolean;
}

type ContentName = { name: string; id?: string };

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-reference-dropdown[mode][language][languages][schemaId]',
    styleUrls: ['./reference-dropdown.component.scss'],
    templateUrl: './reference-dropdown.component.html',
    providers: [
        SQX_REFERENCE_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferenceDropdownComponent extends StatefulControlComponent<State, ReadonlyArray<string> | string> implements OnChanges {
    private readonly contents: ContentDto[] = [];
    private isOpenedBefore = false;
    private isLoadingFailed = false;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public schemaId!: string;

    @Input()
    public mode: 'Array' | 'Single' = 'Single';

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public control = new UntypedFormControl('');

    public get isValid() {
        return !!this.schemaId && !!this.language;
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly contentsResolver: ResolveContents,
        private readonly localizer: LocalizerService,
    ) {
        super(changeDetector, {
            contentNames: [],
        });

        this.own(
            value$(this.control)
                .subscribe((id: string) => {
                    if (this.control.enabled) {
                        if (id) {
                            if (this.mode === 'Single') {
                                this.callChange(id);
                            } else {
                                this.callChange([id]);
                            }
                        } else if (this.mode === 'Single') {
                            this.callChange(null);
                        } else {
                            this.callChange([]);
                        }

                        this.callTouched();
                    }
                }));
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['schemaId']) {
            this.contents.clear();

            this.resetState();
        }

        if (changes['language'] || changes['schemaId']) {
            this.resetContentNames(true);
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.control.disable(NO_EMIT);
        } else if (this.isValid) {
            this.control.enable(NO_EMIT);
        }
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

    public onOpened() {
        if (this.isOpenedBefore) {
            return;
        }

        this.isOpenedBefore = true;
        this.loadMore(this.contentsResolver.resolveAll(this.schemaId));
    }

    private selectContent(id: string | undefined) {
        const isNewId = !this.contents.find(x => x.id === id);

        if (id && isNewId) {
            this.loadMore(this.contentsResolver.resolveMany([id]));
        }

        this.control.setValue(id, NO_EMIT);
    }

    private loadMore(observable: Observable<ContentsDto>) {
        this.next({ isLoading: true });

        observable
            .subscribe({
                next: ({ items }) => {
                    if (items.length === 0) {
                        return;
                    }

                    for (const content of items) {
                        const index = this.contents.findIndex(x => x.id === content.id);

                        if (index >= 0) {
                            this.contents[index] = content;
                        } else {
                            this.contents.push(content);
                        }
                    }

                    this.isLoadingFailed = false;

                    this.resetContentNames(true);
                },
                error: () => {
                    this.isLoadingFailed = true;

                    this.resetContentNames(false);
                },
            });
    }

    private resetContentNames(rebuild: boolean) {
        const success = this.isValid && !this.isLoadingFailed;

        this.onDisabled(!success || this.snapshot.isDisabled);

        if (success && rebuild) {
            const contentNames: ContentName[] = [
                { name: this.localizer.getOrKey('contents.noReference') },
            ];

            for (const content of this.contents) {
                const name =
                    content.referenceFields
                        .map(f => getContentValue(content, this.language, f, false))
                        .map(v => v.formatted)
                        .defined()
                        .join(', ')
                    || this.localizer.getOrKey('common.noValue');

                contentNames.push({ name, id: content.id });
            }

            this.next({ contentNames });
        }

        this.next({ isLoading: false });
    }
}

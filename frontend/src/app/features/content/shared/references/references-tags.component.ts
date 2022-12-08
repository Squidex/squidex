/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NG_VALUE_ACCESSOR, UntypedFormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { Types } from '@app/framework';
import { ContentDto, ContentsDto, LanguageDto, LocalizerService, ResolveContents, StatefulControlComponent } from '@app/shared/internal';
import { ReferencesTagsConverter } from './references-tag-converter';

export const SQX_REFERENCES_TAGS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesTagsComponent), multi: true,
};

interface State {
    // The tags converter.
    converter: ReferencesTagsConverter;

    // True when loading.
    isLoading?: boolean;
}

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-references-tags[language][languages][schemaId]',
    styleUrls: ['./references-tags.component.scss'],
    templateUrl: './references-tags.component.html',
    providers: [
        SQX_REFERENCES_TAGS_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferencesTagsComponent extends StatefulControlComponent<State, ReadonlyArray<string>> implements OnChanges {
    private readonly contents: ContentDto[] = [];
    private isOpenedBefore = false;
    private isLoadingFailed = false;

    @Input()
    public schemaId!: string;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public control = new UntypedFormControl([]);

    public get isValid() {
        return !!this.schemaId && !!this.language;
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly contentsResolver: ResolveContents,
        private readonly localizer: LocalizerService,
    ) {
        super(changeDetector, {
            converter: new ReferencesTagsConverter(null!, [], localizer),
        });

        this.own(
            this.control.valueChanges
                .subscribe((value: string[]) => {
                    if (value && value.length > 0) {
                        this.callTouched();
                        this.callChange(value);
                    } else {
                        this.callTouched();
                        this.callChange(null);
                    }
                }));
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['schemaId']) {
            this.contents.clear();

            this.resetState();
        }

        if (changes['language'] || changes['schemaId']) {
            this.resetConverterState(true);
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.control.disable(NO_EMIT);
        } else if (this.isValid) {
            this.control.enable(NO_EMIT);
        }
    }

    public onOpened() {
        if (this.isOpenedBefore) {
            return;
        }

        this.isOpenedBefore = true;
        this.loadMore(this.contentsResolver.resolveAll(this.schemaId));
    }

    public writeValue(obj: ReadonlyArray<string>) {
        if (Types.isArrayOfString(obj)) {
            this.selectContent(obj);
        } else {
            this.selectContent(undefined);
        }
    }

    private selectContent(ids?: ReadonlyArray<string>) {
        const newIds = ids?.filter(x => !this.contents?.find(y => y.id === x));

        if (newIds && newIds.length > 0) {
            this.loadMore(this.contentsResolver.resolveMany(newIds));
        }

        this.control.setValue(ids, NO_EMIT);
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

                    this.resetConverterState(true);
                },
                error: () => {
                    this.isLoadingFailed = true;

                    this.resetConverterState(false);
                },
            });
    }

    private resetConverterState(rebuild: boolean) {
        const success = this.isValid && !this.isLoadingFailed;

        this.onDisabled(!success || this.snapshot.isDisabled);

        if (rebuild) {
            const converter = new ReferencesTagsConverter(this.language, this.contents, this.localizer);

            this.next({ converter });
        }

        this.next({ isLoading: false });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { ContentDto, ContentsDto, LanguageDto, LocalizerService, ResolveContents, StatefulControlComponent, Subscriptions, TagEditorComponent, TranslatePipe, TypedSimpleChanges, Types } from '@app/shared';
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
    standalone: true,
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
export class ReferencesTagsComponent extends StatefulControlComponent<State, ReadonlyArray<string>> {
    private readonly subscriptions = new Subscriptions();
    private readonly contents: ContentDto[] = [];
    private isOpenedBefore = false;
    private isLoadingFailed = false;

    @Input({ required: true })
    public schemaId!: string;

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public control = new UntypedFormControl([]);

    public get isValid() {
        return !!this.schemaId && !!this.language;
    }

    constructor(
        private readonly contentsResolver: ResolveContents,
        private readonly localizer: LocalizerService,
    ) {
        super({ converter: new ReferencesTagsConverter(null!, [], localizer) });

        this.subscriptions.add(
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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.schemaId) {
            this.contents.clear();

            this.resetState();
        }

        if (changes.language || changes.schemaId) {
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

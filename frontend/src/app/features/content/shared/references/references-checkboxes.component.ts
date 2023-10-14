/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, inject, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR, UntypedFormControl } from '@angular/forms';
import { AppsState, ContentDto, ContentsService, LanguageDto, LocalizerService, StatefulControlComponent, TypedSimpleChanges, UIOptions } from '@app/shared/internal';
import { ReferencesTagsConverter } from './references-tag-converter';

export const SQX_REFERENCES_CHECKBOXES_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesCheckboxesComponent), multi: true,
};

interface State {
    // The tags converter.
    converter: ReferencesTagsConverter;
}

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-references-checkboxes',
    styleUrls: ['./references-checkboxes.component.scss'],
    templateUrl: './references-checkboxes.component.html',
    providers: [
        SQX_REFERENCES_CHECKBOXES_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferencesCheckboxesComponent extends StatefulControlComponent<State, ReadonlyArray<string>> {
    private readonly itemCount: number = inject(UIOptions).value.referencesDropdownItemCount;
    private contentItems: ReadonlyArray<ContentDto> | null = null;

    @Input({ required: true })
    public schemaId: string | undefined | null;

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public control = new UntypedFormControl([]);

    public get isValid() {
        return !!this.schemaId && !!this.language;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService,
    ) {
        super({ converter: new ReferencesTagsConverter(null!, [], localizer) });

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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.schemaId) {
            this.resetState();

            if (this.isValid) {
                this.contentsService.getContents(this.appsState.appName, this.schemaId!, { take: this.itemCount })
                    .subscribe({
                        next: contents => {
                            this.contentItems = contents.items;

                            this.resetConverterState();
                        },
                        error: () => {
                            this.contentItems = null;

                            this.resetConverterState();
                        },
                    });
            } else {
                this.contentItems = null;

                this.resetConverterState();
            }
        } else {
            this.resetConverterState();
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.control.disable(NO_EMIT);
        } else if (this.isValid) {
            this.control.enable(NO_EMIT);
        }
    }

    public writeValue(obj: ReadonlyArray<string>) {
        this.control.setValue(obj, NO_EMIT);
    }

    private resetConverterState() {
        const success = this.isValid && this.contentItems && this.contentItems.length > 0;

        this.onDisabled(!success || this.snapshot.isDisabled);

        let converter: ReferencesTagsConverter;

        if (success) {
            converter = new ReferencesTagsConverter(this.language, this.contentItems!, this.localizer);
        } else {
            converter = new ReferencesTagsConverter(null!, [], this.localizer);
        }

        this.next({ converter });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppsState, ContentDto, ContentsService, LanguageDto, LocalizerService, StatefulControlComponent, UIOptions } from '@app/shared/internal';
import { ReferencesTagsConverter } from './references-tag-converter';

export const SQX_REFERENCES_CHECKBOXES_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesCheckboxesComponent), multi: true
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
        SQX_REFERENCES_CHECKBOXES_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesCheckboxesComponent extends StatefulControlComponent<State, ReadonlyArray<string>> implements OnChanges {
    private readonly itemCount: number;
    private contentItems: ReadonlyArray<ContentDto> | null = null;

    @Input()
    public schemaId: string;

    @Input()
    public language: LanguageDto;

    public get isValid() {
        return !!this.schemaId && !!this.language;
    }

    public selectionControl = new FormControl([]);

    constructor(changeDetector: ChangeDetectorRef, uiOptions: UIOptions,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService
    ) {
        super(changeDetector, { converter: new ReferencesTagsConverter(null!, [], localizer) });

        this.itemCount = uiOptions.get('referencesDropdownItemCount');

        this.own(
            this.selectionControl.valueChanges
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
            this.resetState();

            if (this.isValid) {
                this.contentsService.getContents(this.appsState.appName, this.schemaId, { take: this.itemCount })
                    .subscribe(contents => {
                        this.contentItems = contents.items;

                        this.resetConverterState();
                    }, () => {
                        this.contentItems = null;

                        this.resetConverterState();
                    });
            } else {
                this.contentItems = null;

                this.resetConverterState();
            }
        } else {
            this.resetConverterState();
        }
    }

    public setDisabledState(isDisabled: boolean) {
        if (isDisabled) {
            this.selectionControl.disable(NO_EMIT);
        } else if (this.isValid) {
            this.selectionControl.enable(NO_EMIT);
        }

        super.setDisabledState(isDisabled);
    }

    public writeValue(obj: ReadonlyArray<string>) {
        this.selectionControl.setValue(obj, NO_EMIT);
    }

    private resetConverterState() {
        let converter: ReferencesTagsConverter;

        if (this.isValid && this.contentItems && this.contentItems.length > 0) {
            converter = new ReferencesTagsConverter(this.language, this.contentItems, this.localizer);

            this.selectionControl.enable(NO_EMIT);
        } else {
            converter = new ReferencesTagsConverter(null!, [], this.localizer);

            this.selectionControl.disable(NO_EMIT);
        }

        this.next({ converter });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    AppsState,
    ContentDto,
    ContentsService,
    Converter,
    getContentValue,
    LanguageDto,
    StatefulControlComponent,
    TagValue,
    UIOptions
} from '@app/shared/internal';

export const SQX_REFERENCES_TAGS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesTagsComponent), multi: true
};

const NO_EMIT = { emitEvent: false };

class TagsConverter implements Converter {
    public suggestions: ReadonlyArray<TagValue> = [];

    constructor(language: LanguageDto, contents: ReadonlyArray<ContentDto>) {
        this.suggestions = this.createTags(language, contents);
    }

    public convertInput(input: string) {
        const result = this.suggestions.find(x => x.name === input);

        return result || null;
    }

    public convertValue(value: any) {
        const result = this.suggestions.find(x => x.id === value);

        return result || null;
    }

    private createTags(language: LanguageDto, contents: ReadonlyArray<ContentDto>): ReadonlyArray<TagValue> {
        if (contents.length === 0) {
            return [];
        }

        const values = contents.map(content => {
            const name =
                content.referenceFields
                    .map(f => getContentValue(content, language, f, false))
                    .map(v => v.formatted || 'No value')
                    .filter(v => !!v)
                    .join(', ');

            return new TagValue(content.id, name, content.id);
        });

        return values;
    }
}

interface State {
    converter: TagsConverter;
}

@Component({
    selector: 'sqx-references-tags',
    template: `
        <sqx-tag-editor placeholder=", to add reference" [converter]="snapshot.converter" [formControl]="selectionControl"
            [suggestedValues]="snapshot.converter.suggestions">
        </sqx-tag-editor>
    `,
    styles: [`
        .truncate {
            min-height: 1.5rem;
        }`
    ],
    providers: [SQX_REFERENCES_TAGS_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesTagsComponent extends StatefulControlComponent<State, ReadonlyArray<string>> implements OnChanges {
    private itemCount: number;
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
        private readonly contentsService: ContentsService
    ) {
        super(changeDetector, { converter: new TagsConverter(null!, []) });

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
                this.contentsService.getContents(this.appsState.appName, this.schemaId, this.itemCount, 0)
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
        }
    }

    public setDisabledState(isDisabled: boolean) {
        if (isDisabled) {
            this.selectionControl.disable();
        } else if (this.isValid) {
            this.selectionControl.enable();
        }

        super.setDisabledState(isDisabled);
    }

    public writeValue(obj: ReadonlyArray<string>) {
        this.selectionControl.setValue(obj, NO_EMIT);
    }

    private resetConverterState() {
        let converter: TagsConverter;

        if (this.isValid && this.contentItems && this.contentItems.length > 0) {
            converter = new TagsConverter(this.language, this.contentItems);

            this.selectionControl.enable();
        } else {
            converter = new TagsConverter(null!, []);

            this.selectionControl.disable();
        }

        this.next({ converter });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppsState, ContentDto, ContentsService, getContentValue, LanguageDto, LocalizerService, StatefulControlComponent, Types, UIOptions, value$ } from '@app/shared/internal';

export const SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesDropdownComponent), multi: true,
};

interface State {
    // The referenced content items.
    contents: ReadonlyArray<ContentDto>;

    // The names of the selected content items for search.
    contentNames: ReadonlyArray<ContentName>;

    // The name of the selected item.
    selectedItem?: ContentName;
}

type ContentName = { name: string; id?: string };

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-references-dropdown',
    styleUrls: ['./references-dropdown.component.scss'],
    templateUrl: './references-dropdown.component.html',
    providers: [
        SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferencesDropdownComponent extends StatefulControlComponent<State, ReadonlyArray<string> | string> implements OnChanges {
    private readonly itemCount: number;
    private languageField: LanguageDto;
    private selectedId: string | undefined;

    @Input()
    public schemaId: string;

    @Input()
    public mode: 'Array' | 'Single';

    @Input()
    public set disabled(value: boolean | null | undefined) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set language(value: LanguageDto) {
        this.languageField = value;

        this.next(s => ({
            ...s,
            contentNames: this.createContentNames(s.contents),
        }));
    }

    public get isValid() {
        return !!this.schemaId && !!this.languageField;
    }

    public control = new FormControl('');

    constructor(changeDetector: ChangeDetectorRef, uiOptions: UIOptions,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService,
    ) {
        super(changeDetector, {
            contents: [],
            contentNames: [],
        });

        this.itemCount = uiOptions.get('referencesDropdownItemCount');

        this.own(
            value$(this.control)
                .subscribe((value: ContentName) => {
                    if (this.control.enabled) {
                        if (value && value.id) {
                            if (this.mode === 'Single') {
                                this.callChange(value.id);
                            } else {
                                this.callChange([value.id]);
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
            this.resetState();

            if (this.isValid) {
                this.contentsService.getContents(this.appsState.appName, this.schemaId, { take: this.itemCount })
                    .subscribe(({ items: contents }) => {
                        const contentNames = this.createContentNames(contents);

                        this.next({ contents, contentNames });

                        this.selectContent();
                    }, () => {
                        this.control.disable(NO_EMIT);
                    });
            } else {
                this.control.disable(NO_EMIT);
            }
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
            this.selectedId = obj;

            this.selectContent();
        } else if (Types.isArrayOfString(obj)) {
            this.selectedId = obj[0];

            this.selectContent();
        } else {
            this.selectedId = undefined;

            this.unselectContent();
        }
    }

    private selectContent() {
        this.control.setValue(this.snapshot.contentNames.find(x => x.id === this.selectedId), NO_EMIT);
    }

    private unselectContent() {
        this.control.setValue(undefined, NO_EMIT);
    }

    private createContentNames(contents: ReadonlyArray<ContentDto>): ReadonlyArray<ContentName> {
        if (contents.length === 0) {
            return [];
        }

        const names = contents.map(content => {
            const name =
                content.referenceFields
                    .map(f => getContentValue(content, this.languageField, f, false))
                    .map(v => v.formatted || this.localizer.getOrKey('common.noValue'))
                    .filter(v => !!v)
                    .join(', ');

            return { name, id: content.id };
        });

        return [{ name: this.localizer.getOrKey('contents.noReference') }, ...names];
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppsState, ContentDto, ContentsService, getContentValue, LanguageDto, LocalizerService, StatefulControlComponent, Types, UIOptions, value$ } from '@app/shared/internal';

export const SQX_REFERENCE_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferenceDropdownComponent), multi: true,
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
    selector: 'sqx-reference-dropdown[mode][schemaId]',
    styleUrls: ['./reference-dropdown.component.scss'],
    templateUrl: './reference-dropdown.component.html',
    providers: [
        SQX_REFERENCE_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferenceDropdownComponent extends StatefulControlComponent<State, ReadonlyArray<string> | string> implements OnChanges {
    private readonly itemCount: number;

    @Input()
    public language: LanguageDto;

    @Input()
    public schemaId: string;

    @Input()
    public mode: 'Array' | 'Single' = 'Single';

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public get isValid() {
        return !!this.schemaId && !!this.language;
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
            this.resetState();

            if (this.isValid) {
                this.contentsService.getContents(this.appsState.appName, this.schemaId, { take: this.itemCount })
                    .subscribe({
                        next: ({ items: contents }) => {
                            const contentNames = this.createContentNames(contents);

                            this.next({ contents, contentNames });
                        },
                        error: () => {
                            this.control.disable(NO_EMIT);
                        },
                    });
            } else {
                this.control.disable(NO_EMIT);
            }
        }

        if (changes['language']) {
            this.next(s => ({
                ...s,
                contentNames: this.createContentNames(this.snapshot.contents),
            }));
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

    private selectContent(value: any) {
        this.control.setValue(value, NO_EMIT);
    }

    private createContentNames(contents: ReadonlyArray<ContentDto>): ReadonlyArray<ContentName> {
        if (contents.length === 0) {
            return [];
        }

        const names = contents.map(content => {
            const name =
                content.referenceFields
                    .map(f => getContentValue(content, this.language, f, false))
                    .map(v => v.formatted)
                    .filter(v => !!v)
                    .join(', ')
                || this.localizer.getOrKey('common.noValue');

            return { name, id: content.id };
        });

        return [{ name: this.localizer.getOrKey('contents.noReference') }, ...names];
    }
}

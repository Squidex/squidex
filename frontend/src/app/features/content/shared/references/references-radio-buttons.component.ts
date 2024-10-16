/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, inject, Input } from '@angular/core';
import { FormControl, FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { AppsState, ContentDto, ContentsService, LanguageDto, LocalizerService, RadioGroupComponent, StatefulControlComponent, Subscriptions, TypedSimpleChanges, UIOptions } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

export const SQX_REFERENCES_RADIO_BUTTONS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesRadioButtonsComponent), multi: true,
};

interface State {
    // The tags converter.
    converter: ReferencesTagsConverter;
}

const NO_EMIT = { emitEvent: false };

@Component({
    standalone: true,
    selector: 'sqx-references-radio-buttons',
    styleUrls: ['./references-radio-buttons.component.scss'],
    templateUrl: './references-radio-buttons.component.html',
    providers: [
        SQX_REFERENCES_RADIO_BUTTONS_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        RadioGroupComponent,
        ReactiveFormsModule,
    ],
})
export class ReferencesRadioButtonsComponent extends StatefulControlComponent<State, ReadonlyArray<string> | null | undefined> {
    private readonly subscriptions = new Subscriptions();
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

    public control = new FormControl<string | null | undefined>(undefined);

    public get isValid() {
        return !!this.schemaId && !!this.language;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService,
    ) {
        super({ converter: new ReferencesTagsConverter(null!, [], localizer) });

        this.subscriptions.add(
            this.control.valueChanges
                .subscribe(value => {
                    if (value) {
                        this.callTouched();
                        this.callChange([value]);
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

    public writeValue(obj: ReadonlyArray<string> | null | undefined) {
        this.control.setValue(obj?.[0], NO_EMIT);
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

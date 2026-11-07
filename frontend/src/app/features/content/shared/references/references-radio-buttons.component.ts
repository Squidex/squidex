/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, forwardRef, inject, input, signal } from '@angular/core';
import { FormControl, FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, AppsState, ContentDto, ContentsService, LocalizerService, RadioGroupComponent, SignalControlComponent, UIOptions } from '@app/shared';
import { ReferencesTagsConverter } from './references-tag-converter';

export const SQX_REFERENCES_RADIO_BUTTONS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesRadioButtonsComponent), multi: true,
};

const NO_EMIT = { emitEvent: false };

@Component({
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
export class ReferencesRadioButtonsComponent extends SignalControlComponent<ReadonlyArray<string> | null | undefined> {
    private readonly itemCount: number = inject(UIOptions).value.referencesDropdownItemCount;

    /** The ID of the schema to select the contents from. */
    public readonly schemaId = input.required<string | undefined | null>();

    /** The language to build the names of the contents. */
    public readonly language = input.required<AppLanguageDto>();

    protected readonly control = new FormControl<string | null | undefined>(undefined);

    private readonly contentItems = signal<ReadonlyArray<ContentDto> | null>(null);

    private readonly isValid = computed(() => {
        return !!this.schemaId() && !!this.language();
    });

    private readonly isSuccess = computed(() => {
        const contentItems = this.contentItems();

        return this.isValid() && !!contentItems && contentItems.length > 0;
    });

    protected readonly converter = computed(() => {
        const contentItems = this.contentItems();
        if (!this.isSuccess() || !contentItems) {
            return new ReferencesTagsConverter(null!, [], this.localizer);
        }

        return new ReferencesTagsConverter(this.language(), contentItems, this.localizer);
    });

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService,
    ) {
        super();

        effect(() => {
            if (this.isDisabled() || !this.isSuccess()) {
                this.control.disable(NO_EMIT);
            } else {
                this.control.enable(NO_EMIT);
            }
        });

        effect(onCleanup => {
            const schemaId = this.schemaId();
            if (!schemaId) {
                this.contentItems.set(null);
                return;
            }

            const subscription =
                this.contentsService.getContents(this.appsState.appName, schemaId, { take: this.itemCount })
                    .subscribe({
                        next: contents => {
                            this.contentItems.set(contents.items);
                        },
                        error: () => {
                            this.contentItems.set(null);
                        },
                    });

            onCleanup(() => subscription.unsubscribe());
        });

        effect(onCleanup => {
            const subscription =
                this.control.valueChanges
                    .subscribe(value => {
                        this.callTouched();

                        if (value) {
                            this.callChange([value]);
                        } else {
                            this.callChange(null);
                        }
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public writeValue(obj: ReadonlyArray<string> | null | undefined) {
        this.control.setValue(obj?.[0], NO_EMIT);
    }
}

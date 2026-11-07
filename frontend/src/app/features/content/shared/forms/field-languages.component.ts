/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { AppLanguageDto, FieldForm, LanguageSelectorComponent, TourHintDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-field-languages',
    styleUrls: ['./field-languages.component.scss'],
    templateUrl: './field-languages.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        LanguageSelectorComponent,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class FieldLanguagesComponent {
    /** Emits when the user toggles between showing all languages and a single language. */
    public readonly showAllControlsChange = output<boolean>();

    /** Shows the editors for all languages instead of only the selected language. */
    public readonly showAllControls = input(false, { transform: booleanAttribute });

    /** Emits when the user selects another language. */
    public readonly languageChange = output<AppLanguageDto>();

    /** The selected language. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The form model of the field. */
    public readonly formModel = input.required<FieldForm>();

    public toggleShowAllControls() {
        this.showAllControlsChange.emit(!this.showAllControls());
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { AppLanguageDto, contentTranslationStatus, SchemaDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-translation-status',
    styleUrls: ['./translation-status.component.scss'],
    templateUrl: './translation-status.component.html',
})
export class TranslationStatusComponent {
    /** The content data to compute the translation status for. */
    public readonly data = input.required<any>();

    /** The language to show the status for. Shows the average of all languages when not set. */
    public readonly language = input<AppLanguageDto | null>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The schema of the content. */
    public readonly schema = input.required<SchemaDto | undefined>();

    protected readonly text = computed(() => {
        const schema = this.schema();
        if (!schema) {
            return 'N/A';
        }

        const languages = this.languages();
        const status = contentTranslationStatus(this.data(), schema, languages);

        const language = this.language();
        if (language) {
            return `${status[language.iso2Code] || 0} %`;
        }

        let progress = 0;
        for (const value of Object.values(status)) {
            progress += value;
        }

        return `${Math.round(progress / languages.length) || 0} %`;
    });
}

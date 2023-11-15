/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { contentTranslationStatus, LanguageDto, SchemaDto } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-translation-status',
    styleUrls: ['./translation-status.component.scss'],
    templateUrl: './translation-status.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TranslationStatusComponent {
    @Input({ required: true })
    public data!: any;

    @Input()
    public language?: LanguageDto | null;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public schema?: SchemaDto;

    public text = 'N/A';

    public ngOnChanges() {
        if (!this.schema) {
            this.text = 'N/A';
            return;
        }

        const status = contentTranslationStatus(this.data, this.schema, this.languages);

        let progress = 0;

        if (this.language) {
            progress = status[this.language.iso2Code];
        } else {
            for (const value of Object.values(status)) {
                progress += value;
            }

            progress = Math.round(progress / this.languages.length);
        }

        this.text = `${progress || 0} %`;
    }
}

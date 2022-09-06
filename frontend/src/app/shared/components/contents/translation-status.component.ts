/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { contentTranslationStatus, LanguageDto, SchemaDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-translation-status[data][languages][schema]',
    styleUrls: ['./translation-status.component.scss'],
    templateUrl: './translation-status.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TranslationStatusComponent {
    @Input()
    public data!: any;

    @Input()
    public language?: LanguageDto | null;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
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

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AppLanguageDto, ContentDto, ManualContentsState } from '@app/shared';

@Component({
    selector: 'sqx-content-references',
    styleUrls: ['./content-references.component.scss'],
    templateUrl: './content-references.component.html',
    providers: [
        ManualContentsState
    ]
})
export class ContentReferencesComponent implements OnChanges {
    @Input()
    public content: ContentDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public mode: 'references' | 'referencing' = 'references';

    constructor(
        public readonly contentsState: ManualContentsState
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content'] || changes['mode']) {
            this.contentsState.schema = { name: this.content.schemaName };

            if (this.mode === 'references') {
                this.contentsState.loadReference(this.content.id);
            } else {
                this.contentsState.loadReferencing(this.content.id);
            }
        }
    }

    public trackByContent(_index: number, content: ContentDto) {
        return content.id;
    }
}
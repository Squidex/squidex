/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AppLanguageDto, ContentDto, ManualContentsState, Router2State } from '@app/shared';

@Component({
    selector: 'sqx-content-references',
    styleUrls: ['./content-references.component.scss'],
    templateUrl: './content-references.component.html',
    providers: [
        Router2State, ManualContentsState
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
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ManualContentsState
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content'] || changes['mode']) {
            this.contentsState.schema = { name: this.content.schemaName };

            if (this.mode === 'references') {
                this.contentsState.loadReference(this.content.id, this.contentsRoute);
            } else {
                this.contentsState.loadReferencing(this.content.id, this.contentsRoute);
            }
        }
    }

    public validate() {
        this.contentsState.validate(this.contentsState.snapshot.contents);
    }

    public publish() {
        this.contentsState.changeManyStatus(this.contentsState.snapshot.contents.filter(x => x.canPublish), 'Published');
    }

    public trackByContent(_index: number, content: ContentDto) {
        return content.id;
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { AppLanguageDto, ComponentContentsState, ContentDto, QuerySynchronizer, Router2State } from '@app/shared';
import { ContentPageComponent } from './../content-page.component';

@Component({
    selector: 'sqx-content-references[content][language][languages]',
    styleUrls: ['./content-references.component.scss'],
    templateUrl: './content-references.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        Router2State, ComponentContentsState,
    ],
})
export class ContentReferencesComponent implements OnChanges, OnInit, OnDestroy {
    @Input()
    public content: ContentDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public mode: 'references' | 'referencing' = 'references';

    constructor(
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ComponentContentsState,
        private readonly parent: ContentPageComponent,
    ) {
    }

    public ngOnInit() {
        this.parent.addAction('contents.validate', () => {
            this.contentsState.validate(this.contentsState.snapshot.contents);
        });

        this.parent.addAction('contents.publishAll', () => {
            this.contentsState.changeManyStatus(this.contentsState.snapshot.contents.filter(x => x.canPublish), 'Published');
        });
    }

    public ngOnDestroy() {
        this.parent.clearActions();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content'] || changes['mode']) {
            this.contentsState.schema = { name: this.content.schemaName };

            const initial =
                this.contentsRoute.mapTo(this.contentsState)
                    .withPaging('contents', 10)
                    .withSynchronizer(QuerySynchronizer.INSTANCE)
                    .getInitial();

            if (this.mode === 'references') {
                this.contentsState.loadReference(this.content.id, initial);
            } else {
                this.contentsState.loadReferencing(this.content.id, initial);
            }

            this.contentsRoute.listen();
        }
    }

    public trackByContent(_index: number, content: ContentDto) {
        return content.id;
    }
}

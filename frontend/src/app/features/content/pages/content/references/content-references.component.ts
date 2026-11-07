/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, input } from '@angular/core';
import { AppLanguageDto, ComponentContentsState, ContentDto, ContentsColumnsPipe, ListViewComponent, PagerComponent, QuerySynchronizer, Router2State, ToolbarService, TranslatePipe } from '@app/shared';
import { ReferenceItemComponent } from '../../../shared/references/reference-item.component';

@Component({
    selector: 'sqx-content-references',
    styleUrls: ['./content-references.component.scss'],
    templateUrl: './content-references.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        ComponentContentsState,
        Router2State,
    ],
    imports: [
        ContentsColumnsPipe,
        ListViewComponent,
        PagerComponent,
        ReferenceItemComponent,
        TranslatePipe,
    ],
})
export class ContentReferencesComponent {
    private readonly contentsRoute = inject(Router2State);
    private readonly destroyRef = inject(DestroyRef);
    private readonly toolbar = inject(ToolbarService);

    protected readonly contentsState = inject(ComponentContentsState);

    /** The content to show the references for. */
    public readonly content = input.required<ContentDto>();

    /** The language to render the contents in. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the contents referenced by the content or the contents referencing the content. */
    public readonly mode = input<'references' | 'referencing'>('references');

    constructor() {
        this.toolbar.addButton(this, 'i18n:contents.validate', () => {
            this.validate();
        });

        this.toolbar.addButton(this, 'i18n:contents.publishAll', () => {
            this.publishAll();
        });

        this.destroyRef.onDestroy(() => {
            this.toolbar.remove(this);
        });

        effect(() => {
            const content = this.content();

            this.contentsState.schema = { name: content.schemaName };

            const initial =
                this.contentsRoute.mapTo(this.contentsState)
                    .withPaging('contents', 10)
                    .withSynchronizer(QuerySynchronizer.INSTANCE)
                    .getInitial();

            if (this.mode() === 'references') {
                this.contentsState.loadReferences(content.id, initial);
            } else {
                this.contentsState.loadReferencing(content.id, initial);
            }

            this.contentsRoute.listen();
        });
    }

    private validate() {
        this.contentsState.validate(this.contentsState.snapshot.contents);
    }

    private publishAll() {
        this.contentsState.changeManyStatus(this.contentsState.snapshot.contents.filter(x => x.canPublish), 'Published');
    }
}

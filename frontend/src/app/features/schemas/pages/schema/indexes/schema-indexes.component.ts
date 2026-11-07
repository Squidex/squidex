/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input } from '@angular/core';
import { switchMap, timer } from 'rxjs';
import { CodeComponent, DialogModel, IndexesState, LanguagesState, ListViewComponent, MarkdownDirective, ModalDirective, SchemaDto, TranslatePipe, UIOptions } from '@app/shared';
import { IndexFormComponent } from './index-form.component';
import { IndexComponent } from './index.component';

@Component({
    selector: 'sqx-schema-indexes',
    styleUrls: ['./schema-indexes.component.scss'],
    templateUrl: './schema-indexes.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        CodeComponent,
        IndexFormComponent,
        IndexComponent,
        MarkdownDirective,
        ModalDirective,
        ListViewComponent,
        TranslatePipe,
    ],
})
export class SchemaIndexesComponent {
    public readonly canCreateIndexes = inject(UIOptions).value.canCreateIndexes;

    /** The schema to manage the indexes for. */
    public readonly schema = input.required<SchemaDto>();

    public readonly addIndexDialog = new DialogModel();

    constructor(
        public readonly indexesState: IndexesState,
        public readonly languagesState: LanguagesState,
    ) {
        this.indexesState.load();

        effect(onCleanup => {
            const subscription =
                timer(3000, 3000).pipe(
                    switchMap(() => this.indexesState.load(false, true)))
                .subscribe();

            onCleanup(() => subscription.unsubscribe());
        });

        this.languagesState.load();
    }
}

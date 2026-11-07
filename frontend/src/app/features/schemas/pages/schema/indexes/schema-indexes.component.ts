/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
    protected readonly indexesState = inject(IndexesState);
    protected readonly languagesState = inject(LanguagesState);
    protected readonly canCreateIndexes = inject(UIOptions).value.canCreateIndexes;

    /** The schema to manage the indexes for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly addIndexDialog = new DialogModel();

    constructor() {
        this.indexesState.load();

        timer(3000, 3000).pipe(
            switchMap(() => this.indexesState.load(false, true)),
            takeUntilDestroyed())
            .subscribe();

        this.languagesState.load();
    }
}

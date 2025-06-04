/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AsyncPipe } from '@angular/common';
import { Component, inject, Input, OnInit } from '@angular/core';
import { switchMap, timer } from 'rxjs';
import { CodeComponent, DialogModel, IndexesState, LanguagesState, ListViewComponent, MarkdownDirective, ModalDirective, SchemaDto, Subscriptions, TranslatePipe, UIOptions } from '@app/shared';
import { IndexFormComponent } from './index-form.component';
import { IndexComponent } from './index.component';

@Component({
    selector: 'sqx-schema-indexes',
    styleUrls: ['./schema-indexes.component.scss'],
    templateUrl: './schema-indexes.component.html',
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
export class SchemaIndexesComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public readonly canCreateIndexes = inject(UIOptions).value.canCreateIndexes;

    @Input({ required: true })
    public schema!: SchemaDto;

    public addIndexDialog = new DialogModel();

    constructor(
        public readonly indexesState: IndexesState,
        public readonly languagesState: LanguagesState,
    ) {
    }

    public ngOnInit() {
        this.indexesState.load();

        this.subscriptions.add(
            timer(3000, 3000).pipe(
                switchMap(() => this.indexesState.load(false, true))));

        this.languagesState.load();
    }
}
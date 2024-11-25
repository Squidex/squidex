/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AsyncPipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { switchMap, timer } from 'rxjs';
import { DialogModel, IndexesState, LanguagesState, ListViewComponent, ModalDirective, SchemaDto, Subscriptions, TranslatePipe } from '@app/shared';
import { IndexFormComponent } from './index-form.component';
import { IndexComponent } from './index.component';

@Component({
    standalone: true,
    selector: 'sqx-schema-indexes',
    styleUrls: ['./schema-indexes.component.scss'],
    templateUrl: './schema-indexes.component.html',
    imports: [
        AsyncPipe,
        IndexFormComponent,
        IndexComponent,
        ModalDirective,
        ListViewComponent,
        TranslatePipe,
    ],
})
export class SchemaIndexesComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

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
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { onErrorResumeNext, switchMap, tap } from 'rxjs/operators';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsState,
    ImmutableArray,
    LanguagesState,
    ModalModel,
    Queries,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState,
    UIState
} from '@app/shared';

import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html'
})
export class ContentsPageComponent extends ResourceOwner implements OnInit {
    public schema: SchemaDetailsDto;
    public schemaQueries: Queries;

    public searchModal = new ModalModel();

    public selectedItems:  { [id: string]: boolean; } = {};
    public selectionCount = 0;

    public canUnpublish = false;
    public canPublish = false;

    public language: AppLanguageDto;
    public languages: ImmutableArray<AppLanguageDto>;

    public isAllSelected = false;

    @ViewChild('dueTimeSelector')
    public dueTimeSelector: DueTimeSelectorComponent;

    constructor(
        public readonly appsState: AppsState,
        public readonly contentsState: ContentsState,
        private readonly languagesState: LanguagesState,
        private readonly schemasState: SchemasState,
        private readonly uiState: UIState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.resetSelection();

                    this.schema = schema!;
                    this.schemaQueries = new Queries(this.uiState, `schemas.${this.schema.name}`);

                    this.contentsState.init().pipe(onErrorResumeNext()).subscribe();
                }));

        this.own(
            this.contentsState.contents
                .subscribe(() => {
                    this.updateSelectionSummary();
                }));

        this.own(
            this.languagesState.languages
                .subscribe(languages => {
                    this.languages = languages.map(x => x.language);
                    this.language = this.languages.at(0);
                }));
    }

    public reload() {
        this.contentsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public deleteSelected() {
        this.contentsState.deleteMany(this.selectItems()).pipe(onErrorResumeNext()).subscribe();
    }

    public delete(content: ContentDto) {
        this.contentsState.deleteMany([content]).pipe(onErrorResumeNext()).subscribe();
    }

    public publish(content: ContentDto) {
        this.changeContentItems([content], 'Publish');
    }

    public publishSelected() {
        this.changeContentItems(this.selectItems(c => c.status !== 'Published'), 'Publish');
    }

    public unpublish(content: ContentDto) {
        this.changeContentItems([content], 'Unpublish');
    }

    public unpublishSelected() {
        this.changeContentItems(this.selectItems(c => c.status === 'Published'), 'Unpublish');
    }

    public archive(content: ContentDto) {
        this.changeContentItems([content], 'Archive');
    }

    public archiveSelected() {
        this.changeContentItems(this.selectItems(), 'Archive');
    }

    public restore(content: ContentDto) {
        this.changeContentItems([content], 'Restore');
    }

    public restoreSelected() {
        this.changeContentItems(this.selectItems(), 'Restore');
    }

    public clone(content: ContentDto) {
        this.contentsState.create(content.dataDraft, false).pipe(onErrorResumeNext()).subscribe();
    }

    private changeContentItems(contents: ContentDto[], action: string) {
        if (contents.length === 0) {
            return;
        }

        this.dueTimeSelector.selectDueTime(action).pipe(
                tap(() => {
                    this.resetSelection();
                }),
                switchMap(d => this.contentsState.changeManyStatus(contents, action, d)),
                onErrorResumeNext())
            .subscribe();
    }

    public goArchive(isArchive: boolean) {
        this.contentsState.goArchive(isArchive).pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.contentsState.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.contentsState.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public search(query: string) {
        this.contentsState.search(query).pipe(onErrorResumeNext()).subscribe();
    }

    public selectLanguage(language: AppLanguageDto) {
        this.language = language;
    }

    public isItemSelected(content: ContentDto): boolean {
        return !!this.selectedItems[content.id];
    }

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.values.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }

    public selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems[content.id] = isSelected;

        this.updateSelectionSummary();
    }

    private resetSelection() {
        this.selectedItems = {};

        this.updateSelectionSummary();
    }

    public selectAll(isSelected: boolean) {
        this.selectedItems = {};

        if (isSelected) {
            for (let content of this.contentsState.snapshot.contents.values) {
                this.selectedItems[content.id] = true;
            }
        }

        this.updateSelectionSummary();
    }

    public trackByContent(index: number, content: ContentDto): string {
        return content.id;
    }

    private updateSelectionSummary() {
        this.isAllSelected = this.contentsState.snapshot.contents.length > 0;

        this.selectionCount = 0;

        this.canPublish = true;
        this.canUnpublish = true;

        for (let content of this.contentsState.snapshot.contents.values) {
            if (this.selectedItems[content.id]) {
                this.selectionCount++;

                if (content.status !== 'Published') {
                    this.canUnpublish = false;
                }

                if (content.status === 'Published') {
                    this.canPublish = false;
                }
            } else {
                this.isAllSelected = false;
            }
        }
    }
}


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsState,
    ImmutableArray,
    LanguagesState,
    ModalView,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html'
})
export class ContentsPageComponent implements OnDestroy, OnInit {
    private contentsSubscription: Subscription;
    private languagesSubscription: Subscription;
    private selectedSchemaSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public searchModal = new ModalView();

    public dueTimeDialog = new ModalView();
    public dueTime: string | null = '';
    public dueTimeFunction: Function | null;
    public dueTimeAction: string | null = '';
    public dueTimeMode = 'Immediately';

    public selectedItems:  { [id: string]: boolean; } = {};
    public selectionCount = 0;

    public canUnpublish = false;
    public canPublish = false;

    public language: AppLanguageDto;
    public languages: ImmutableArray<AppLanguageDto>;

    public isAllSelected = false;

    constructor(
        public readonly appsState: AppsState,
        public readonly contentsState: ContentsState,
        private readonly languagesState: LanguagesState,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnDestroy() {
        this.contentsSubscription.unsubscribe();
        this.languagesSubscription.unsubscribe();
        this.selectedSchemaSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.selectedSchemaSubscription =
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.resetSelection();

                    this.schema = schema!;

                    this.contentsState.init().onErrorResumeNext().subscribe();
                });

        this.contentsSubscription =
            this.contentsState.contents
                .subscribe(() => {
                    this.updateSelectionSummary();
                });

        this.languagesSubscription =
            this.languagesState.languages
                .subscribe(languages => {
                    this.languages = languages.map(x => x.language);
                    this.language = this.languages.at(0);
                });
    }

    public reload() {
        this.contentsState.load(true).onErrorResumeNext().subscribe();
    }

    public publish(content: ContentDto) {
        this.changeContentItems([content], 'Publish', false);
    }

    public publishSelected(scheduled: boolean) {
        this.changeContentItems(this.s(c => c.status !== 'Published'), 'Publish', false);
    }

    public unpublish(content: ContentDto) {
        this.changeContentItems([content], 'Unpublish', false);
    }

    public unpublishSelected(scheduled: boolean) {
        this.changeContentItems(this.s(c => c.status === 'Published'), 'Unpublish', false);
    }

    public archive(content: ContentDto) {
        this.changeContentItems([content], 'Archive', true);
    }

    public archiveSelected(scheduled: boolean) {
        this.changeContentItems(this.s(), 'Archive', true);
    }

    public restore(content: ContentDto) {
        this.changeContentItems([content], 'Restore', true);
    }

    public restoreSelected(scheduled: boolean) {
        this.changeContentItems(this.s(), 'Restore', true);
    }

    private changeContentItems(contents: ContentDto[], action: string, reload: boolean) {
        if (contents.length === 0) {
            return;
        }

        this.dueTimeFunction = () => {
            this.resetSelection();

            this.contentsState.changeStatus(contents, action, this.dueTime).onErrorResumeNext().subscribe();
        };

        this.dueTimeAction = action;
        this.dueTimeDialog.show();
    }

    public deleteSelected() {
        this.resetSelection();

        this.contentsState.delete(this.s()).onErrorResumeNext().subscribe();
    }

    public delete(content: ContentDto) {
        this.resetSelection();

        this.contentsState.delete([content]).onErrorResumeNext().subscribe();
    }

    public goArchive(isArchive: boolean) {
        this.resetSelection();

        this.contentsState.goArchive(isArchive).onErrorResumeNext().subscribe();
    }

    public goPrev() {
        this.resetSelection();

        this.contentsState.goPrev().onErrorResumeNext().subscribe();
    }

    public goNext() {
        this.resetSelection();

        this.contentsState.goNext().onErrorResumeNext().subscribe();
    }

    public search(query: string) {
        this.resetSelection();

        this.contentsState.search(query).onErrorResumeNext().subscribe();
    }

    public isItemSelected(content: ContentDto): boolean {
        return !!this.selectedItems[content.id];
    }

    public selectLanguage(language: AppLanguageDto) {
        this.language = language;
    }

    public selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems[content.id] = isSelected;

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

    public confirmStatusChange() {
        this.dueTimeFunction!();
        this.dueTimeFunction = null;
        this.dueTimeMode = 'Immediately';
        this.dueTimeDialog.hide();
        this.dueTime = null;
    }

    public cancelStatusChange() {
        this.dueTimeMode = 'Immediately';
        this.dueTimeDialog.hide();
        this.dueTimeFunction = null;
        this.dueTime = null;
    }

    public trackByContent(content: ContentDto): string {
        return content.id;
    }

    private s(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.values.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }

    private resetSelection() {
        this.selectedItems = {};

        this.updateSelectionSummary();
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


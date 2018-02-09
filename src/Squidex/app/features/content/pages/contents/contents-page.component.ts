/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import {
    ContentCreated,
    ContentRemoved,
    ContentStatusChanged,
    ContentUpdated
} from './../messages';

import {
    allData,
    AppContext,
    AppLanguageDto,
    ContentDto,
    ContentsService,
    FieldDto,
    ImmutableArray,
    ModalView,
    Pager,
    SchemaDetailsDto,
    DateTime
} from 'shared';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html',
    providers: [
        AppContext
    ]
})
export class ContentsPageComponent implements OnDestroy, OnInit {
    private contentCreatedSubscription: Subscription;
    private contentUpdatedSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public searchModal = new ModalView();

    public contentItems: ImmutableArray<ContentDto>;
    public contentFields: FieldDto[];
    public contentsFilter = new FormControl();
    public contentsQuery = '';
    public contentsPager = new Pager(0);

    public selectedItems:  { [id: string]: boolean; } = {};
    public selectionCount = 0;

    public canUnpublish = false;
    public canPublish = false;

    public languages: AppLanguageDto[] = [];
    public languageSelected: AppLanguageDto;
    public languageParameter: string;

    public isAllSelected = false;
    public isReadOnly = false;
    public isArchive = false;

    constructor(public readonly ctx: AppContext,
        private readonly contentsService: ContentsService
    ) {
    }

    public ngOnDestroy() {
        this.contentCreatedSubscription.unsubscribe();
        this.contentUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.contentCreatedSubscription =
            this.ctx.bus.of(ContentCreated)
                .subscribe(message => {
                    this.contentItems = this.contentItems.pushFront(message.content);
                    this.contentsPager = this.contentsPager.incrementCount();
                });

        this.contentUpdatedSubscription =
            this.ctx.bus.of(ContentUpdated)
                .subscribe(message => {
                    this.contentItems = this.contentItems.replaceBy('id', message.content, (o, n) => o.update(n.data, n.lastModifiedBy, n.version, n.lastModified));
                });

        const routeData = allData(this.ctx.route);

        this.languages = routeData.appLanguages;

        this.ctx.route.data.map(p => p.isReadOnly)
            .subscribe(isReadOnly => {
                this.isReadOnly = isReadOnly;
            });

        this.ctx.route.params.map(p => p.language)
            .subscribe(language => {
                this.languageSelected = this.languages.find(l => l.iso2Code === language) || this.languages.find(l => l.isMaster) || this.languages[0];
            });

        this.ctx.route.data.map(d => d.schema)
            .subscribe(schema => {
                this.schema = schema;

                this.resetContents();
                this.load();
            });
    }

    public dropData(content: ContentDto) {
        return { content, schemaId: this.schema.id };
    }

    public publishContent(content: ContentDto) {
        this.changeContentItem(content, 'publish', 'Published').subscribe();
    }

    public publishSelected() {
        Observable.forkJoin(
            this.contentItems.values
                .filter(c => this.selectedItems[c.id])
                .filter(c => c.status !== 'Published')
                .map(c => this.changeContentItem(c, 'publish', 'Published')))
            .finally(() => {
                this.updateSelectionSummary();
            })
            .subscribe();
    }

    public unpublishContent(content: ContentDto) {
        this.changeContentItem(content, 'unpublish', 'Draft').subscribe();
    }

    public unpublishSelected() {
        Observable.forkJoin(
            this.contentItems.values
                .filter(c => this.selectedItems[c.id])
                .filter(c => c.status !== 'Unpublished')
                .map(c => this.changeContentItem(c, 'unpublish', 'Draft')))
            .finally(() => {
                this.updateSelectionSummary();
            })
            .subscribe();
    }

    private changeContentItem(content: ContentDto, action: string, status: string): Observable<any> {
        return this.contentsService.changeContentStatus(this.ctx.appName, this.schema.name, content.id, action, content.version)
            .catch(error => {
                this.ctx.notifyError(error);

                return Observable.throw(error);
            })
            .do(dto => {
                this.contentItems = this.contentItems.replaceBy('id', content.changeStatus(status, this.ctx.userToken, dto.version));

                this.emitContentStatusChanged(content);
            });
    }

    public archiveSelected() {
        Observable.forkJoin(
            this.contentItems.values.filter(c => this.selectedItems[c.id])
                .map(c => this.changeContentItem(c, 'archive', 'Archived')))
            .finally(() => {
                this.load();
            })
            .subscribe();
    }

    public archiveContent(content: ContentDto) {
        this.changeContentItem(content, 'archive', 'Archived')
            .finally(() => {
                this.load();
            })
            .subscribe();
    }

    public restoreSelected() {
        Observable.forkJoin(
            this.contentItems.values.filter(c => this.selectedItems[c.id])
                .map(c => this.changeContentItem(c, 'restore', 'Draft')))
            .finally(() => {
                this.load();
            })
            .subscribe();
    }

    public restoreContent(content: ContentDto) {
        this.changeContentItem(content, 'restore', 'Draft')
            .finally(() => {
                this.load();
            })
            .subscribe();
    }

    public deleteSelected(content: ContentDto) {
        Observable.forkJoin(
            this.contentItems.values.filter(c => this.selectedItems[c.id])
                .map(c => this.deleteContentItem(c)))
            .finally(() => {
                this.load();
            })
            .subscribe();
    }

    public deleteContent(content: ContentDto) {
        this.deleteContentItem(content)
            .finally(() => {
                this.load();
            })
            .subscribe();
    }

    public deleteContentItem(content: ContentDto): Observable<any> {
        return this.contentsService.deleteContent(this.ctx.appName, this.schema.name, content.id, content.version)
            .do(() => {
                this.emitContentRemoved(content);
            })
            .catch(error => {
                this.ctx.notifyError(error);

                return Observable.throw(error);
            });
    }

    public load(showInfo = false) {
        this.contentsService.getContents(this.ctx.appName, this.schema.name, this.contentsPager.pageSize, this.contentsPager.skip, this.contentsQuery, undefined, this.isArchive)
            .finally(() => {
                this.selectedItems = {};

                this.updateSelectionSummary();
            })
            .subscribe(dtos => {
                this.contentItems = ImmutableArray.of(dtos.items);
                this.contentsPager = this.contentsPager.setCount(dtos.total);

                if (showInfo) {
                    this.ctx.notifyInfo('Contents reloaded.');
                }
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public updateArchive(isArchive: boolean) {
        this.contentsQuery = this.contentsFilter.value;
        this.contentsPager = new Pager(0);

        this.isArchive = isArchive;

        this.searchModal.hide();

        this.load();
    }

    public search() {
        this.contentsQuery = this.contentsFilter.value;
        this.contentsPager = new Pager(0);

        this.load();
    }

    public goNext() {
        this.contentsPager = this.contentsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.contentsPager = this.contentsPager.goPrev();

        this.load();
    }

    public isItemSelected(content: ContentDto): boolean {
        return !!this.selectedItems[content.id];
    }

    public selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems[content.id] = isSelected;

        this.updateSelectionSummary();
    }

    public selectAll(isSelected: boolean) {
        this.selectedItems = {};

        if (isSelected) {
            for (let c of this.contentItems.values) {
                this.selectedItems[c.id] = true;
            }
        }

        this.updateSelectionSummary();
    }

    private updateSelectionSummary() {
        this.isAllSelected = this.contentItems.length > 0;
        this.selectionCount = 0;
        this.canPublish = true;
        this.canUnpublish = true;

        for (let c of this.contentItems.values) {
            if (this.selectedItems[c.id]) {
                this.selectionCount++;

                if (c.status !== 'Published') {
                    this.canUnpublish = false;
                }

                if (c.status === 'Published') {
                    this.canPublish = false;
                }
            } else {
                this.isAllSelected = false;
            }
        }
    }

    public selectLanguage(language: AppLanguageDto) {
        this.languageSelected = language;
    }

    private emitContentStatusChanged(content: ContentDto) {
        this.ctx.bus.emit(new ContentStatusChanged(content));
    }

    private emitContentRemoved(content: ContentDto) {
        this.ctx.bus.emit(new ContentRemoved(content));
    }

    private resetContents() {
        this.contentItems = ImmutableArray.empty<ContentDto>();
        this.contentsQuery = '';
        this.contentsFilter.setValue('');
        this.contentsPager = new Pager(0);
        this.selectedItems = {};

        this.updateSelectionSummary();
        this.loadFields();
    }

    private loadFields() {
        this.contentFields = this.schema.fields.filter(x => x.properties.isListField);

        if (this.contentFields.length === 0 && this.schema.fields.length > 0) {
            this.contentFields = [this.schema.fields[0]];
        }

        if (this.contentFields.length === 0) {
            this.contentFields = [<any>{}];
        }
    }
}


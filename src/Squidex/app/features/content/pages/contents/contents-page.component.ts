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
    DateTime,
    FieldDto,
    ImmutableArray,
    ModalView,
    Pager,
    SchemaDetailsDto,
    Versioned
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

    public dueTimeDialog = new ModalView();
    public dueTime: string | null = '';
    public dueTimeFunction: Function | null;
    public dueTimeAction: string | null = '';
    public dueTimeMode = 'Immediately';

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
        this.changeContentItems([content], 'Publish', 'Published', false);
    }

    public publishSelected(scheduled: boolean) {
        const contents = this.contentItems.filter(c => c.status !== 'Published' && this.selectedItems[c.id]).values;

        this.changeContentItems(contents, 'Publish', 'Published', false);
    }

    public unpublishContent(content: ContentDto) {
        this.changeContentItems([content], 'Unpublish', 'Draft', false);
    }

    public unpublishSelected(scheduled: boolean) {
        const contents = this.contentItems.filter(c => c.status === 'Published' && this.selectedItems[c.id]).values;

        this.changeContentItems(contents, 'Unpublish', 'Draft', false);
    }

    public archiveContent(content: ContentDto) {
        this.changeContentItems([content], 'Archive', 'Archived', true);
    }

    public archiveSelected(scheduled: boolean) {
        const contents = this.contentItems.filter(c => this.selectedItems[c.id]).values;

        this.changeContentItems(contents, 'Archive', 'Archived', true);
    }

    public restoreContent(content: ContentDto) {
        this.changeContentItems([content], 'Restore', 'Draft', true);
    }

    public restoreSelected(scheduled: boolean) {
        const contents = this.contentItems.filter(c => this.selectedItems[c.id]).values;

        this.changeContentItems(contents, 'Restore', 'Draft', true);
    }

    private changeContentItems(contents: ContentDto[], action: string, status: string, reload: boolean) {
        if (contents.length === 0) {
            return;
        }

        this.dueTimeFunction = () => {
            if (this.dueTime) {
                reload = false;
            }
            Observable.forkJoin(
                contents
                    .map(c => this.changeContentItem(c, action, status, this.dueTime, reload)))
                .finally(() => {
                    if (reload) {
                        this.load();
                    } else {
                        this.updateSelectionSummary();
                    }
                })
                .subscribe();
        };

        this.dueTimeAction = action;
        this.dueTimeDialog.show();
    }

    private changeContentItem(content: ContentDto, action: string, status: string, dueTime: string | null, reload: boolean): Observable<any> {
        return this.contentsService.changeContentStatus(this.ctx.appName, this.schema.name, content.id, action, dueTime, content.version)
            .catch(error => {
                this.ctx.notifyError(error);

                return Observable.throw(error);
            })
            .do(dto => {
                if (!reload) {
                    const dt =
                        dueTime ?
                            DateTime.parseISO_UTC(dueTime) :
                            null;

                    content = content.changeStatus(status, dt, this.ctx.userToken, dto.version);

                    this.contentItems = this.contentItems.replaceBy('id', content);

                    this.emitContentStatusChanged(content);
                }
            });
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

    public onContentSaved(content: ContentDto, update: Versioned<any>) {
        content = content.update(update.payload, this.ctx.userToken, update.version);

        this.contentItems = this.contentItems.replaceBy('id', content);

        this.emitContentUpdated(content);
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

    private emitContentUpdated(content: ContentDto) {
        this.ctx.bus.emit(new ContentUpdated(content));
    }

    private emitContentRemoved(content: ContentDto) {
        this.ctx.bus.emit(new ContentRemoved(content));
    }

    public trackBy(content: ContentDto): string {
        return content.id;
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

    public confirmStatusChange() {
        this.dueTimeFunction!();

        this.cancelStatusChange();
    }

    public cancelStatusChange() {
        this.dueTimeMode = 'Immediately';
        this.dueTimeDialog.hide();
        this.dueTimeFunction = null;
        this.dueTime = null;
    }
}


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    ContentCreated,
    ContentPublished,
    ContentRemoved,
    ContentUnpublished,
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
    SchemaDetailsDto
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

    public languages: AppLanguageDto[] = [];
    public languageSelected: AppLanguageDto;
    public languageParameter: string;

    public isReadOnly = false;
    public isArchive = false;

    public columnWidth: number;

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
        this.contentsService.publishContent(this.ctx.appName, this.schema.name, content.id, content.version)
            .subscribe(dto => {
                content = content.publish(this.ctx.userToken, dto.version);

                this.contentItems = this.contentItems.replaceBy('id', content);

                this.emitContentPublished(content);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public unpublishContent(content: ContentDto) {
        this.contentsService.unpublishContent(this.ctx.appName, this.schema.name, content.id, content.version)
            .subscribe(dto => {
                content = content.unpublish(this.ctx.userToken, dto.version);

                this.contentItems = this.contentItems.replaceBy('id', content);

                this.emitContentUnpublished(content);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public archiveContent(content: ContentDto) {
        this.contentsService.archiveContent(this.ctx.appName, this.schema.name, content.id, content.version)
            .subscribe(dto => {
                content = content.archive(this.ctx.userToken, dto.version);

                this.removeContent(content);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public restoreContent(content: ContentDto) {
        this.contentsService.restoreContent(this.ctx.appName, this.schema.name, content.id, content.version)
            .subscribe(dto => {
                content = content.restore(this.ctx.userToken, dto.version);

                this.removeContent(content);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public deleteContent(content: ContentDto) {
        this.contentsService.deleteContent(this.ctx.appName, this.schema.name, content.id, content.version)
            .subscribe(() => {
                this.removeContent(content);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public load(showInfo = false) {
        this.contentsService.getContents(this.ctx.appName, this.schema.name, this.contentsPager.pageSize, this.contentsPager.skip, this.contentsQuery, undefined, this.isArchive)
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

    public selectLanguage(language: AppLanguageDto) {
        this.languageSelected = language;
    }

    private emitContentPublished(content: ContentDto) {
        this.ctx.bus.emit(new ContentPublished(content));
    }

    private emitContentUnpublished(content: ContentDto) {
        this.ctx.bus.emit(new ContentUnpublished(content));
    }

    private emitContentRemoved(content: ContentDto) {
        this.ctx.bus.emit(new ContentRemoved(content));
    }

    private resetContents() {
        this.contentItems = ImmutableArray.empty<ContentDto>();
        this.contentsQuery = '';
        this.contentsFilter.setValue('');
        this.contentsPager = new Pager(0);

        this.loadFields();
    }

    private removeContent(content: ContentDto) {
        this.contentItems = this.contentItems.removeAll(x => x.id === content.id);
        this.contentsPager = this.contentsPager.decrementCount();

        this.emitContentRemoved(content);
    }

    private loadFields() {
        this.contentFields = this.schema.fields.filter(x => x.properties.isListField);

        if (this.contentFields.length === 0 && this.schema.fields.length > 0) {
            this.contentFields = [this.schema.fields[0]];
        }

        if (this.contentFields.length === 0) {
            this.contentFields = [<any>{}];
        }

        if (this.contentFields.length > 0) {
            this.columnWidth = 100 / this.contentFields.length;
        } else {
            this.columnWidth = 100;
        }
    }
}


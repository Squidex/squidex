/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: triple-equals

import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable, timer } from 'rxjs';
import { filter, onErrorResumeNext, switchMap } from 'rxjs/operators';

import {
    AppsState,
    ContentDto,
    ContentsState,
    fadeAnimation,
    HistoryEventDto,
    HistoryService,
    ModalModel,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState,
    switchSafe
} from '@app/shared';

import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';
import { ContentPageComponent } from './content-page.component';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./content-history-page.component.scss'],
    templateUrl: './content-history-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentHistoryPageComponent extends ResourceOwner implements OnInit {
    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector: DueTimeSelectorComponent;

    public schema: SchemaDetailsDto;

    public content: ContentDto;
    public contentEvents: Observable<ReadonlyArray<HistoryEventDto>>;

    public dropdown = new ModalModel();
    public dropdownNew = new ModalModel();

    constructor(
        private readonly appsState: AppsState,
        private readonly contentPage: ContentPageComponent,
        private readonly contentsState: ContentsState,
        private readonly historyService: HistoryService,
        private readonly schemasState: SchemasState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;
                }));

        this.own(
            this.contentsState.selectedContent
                .subscribe(content => {
                    if (content) {
                        const channel = `contents.${content.id}`;

                        this.contentEvents =
                            timer(0, 5000).pipe(
                                switchSafe((() =>  this.historyService.getHistory(this.appsState.appName, channel))));

                        this.content = content;
                    }
                }));
    }

    public changeStatus(status: string) {
        this.contentPage.checkPendingChanges('change the status').pipe(
                filter(x => !!x),
                switchMap(_ => this.dueTimeSelector.selectDueTime(status)),
                switchMap(d => this.contentsState.changeStatus(this.content, status, d)),
                onErrorResumeNext())
            .subscribe();
    }

    public createDraft() {
        this.contentsState.createDraft(this.content);
    }

    public delete() {
        this.contentsState.deleteMany([this.content]);
    }

    public deleteDraft() {
        this.contentsState.deleteDraft(this.content);
    }

    public loadVersion(event: HistoryEventDto) {
        this.contentPage.loadVersion(event.version, false);
    }

    public compareVersion(event: HistoryEventDto) {
        this.contentPage.loadVersion(event.version, true);
    }

    public trackByEvent(index: number, event: HistoryEventDto) {
        return event.eventId;
    }
}
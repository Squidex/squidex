/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable, timer } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppsState, ContentDto, ContentsState, defined, HistoryEventDto, HistoryService, ModalModel, ResourceOwner, SchemasState, switchSafe } from '@app/shared';
import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';
import { ContentPageComponent } from './content-page.component';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./content-history-page.component.scss'],
    templateUrl: './content-history-page.component.html',
})
export class ContentHistoryPageComponent extends ResourceOwner implements OnInit {
    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector!: DueTimeSelectorComponent;

    public content!: ContentDto;
    public contentEvents?: Observable<ReadonlyArray<HistoryEventDto>>;

    public dropdown = new ModalModel();
    public dropdownNew = new ModalModel();

    public get disableScheduler() {
        return this.appsState.snapshot.selectedSettings?.hideScheduler === true;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contentPage: ContentPageComponent,
        private readonly contentsState: ContentsState,
        private readonly historyService: HistoryService,
        private readonly schemasState: SchemasState,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.contentsState.selectedContent.pipe(defined())
                .subscribe(content => {
                    this.content = content;
                }));

        this.contentEvents =
            this.contentsState.selectedContent.pipe(
                defined(),
                map(content => `schemas.${this.schemasState.schemaId}.contents.${content.id}`),
                switchSafe(channel => timer(0, 5000).pipe(map(() => channel))),
                switchSafe(channel => this.historyService.getHistory(this.appsState.appName, channel)));
    }

    public changeStatus(status: string) {
        this.contentPage.checkPendingChangesBeforeChangingStatus().pipe(
                defined(),
                switchSafe(_ => this.dueTimeSelector.selectDueTime(status)),
                switchSafe(d => this.contentsState.changeManyStatus([this.content], status, d)))
            .subscribe();
    }

    public createDraft() {
        this.contentPage.checkPendingChangesBeforeChangingStatus().pipe(
                defined(),
                switchSafe(() => this.contentsState.createDraft(this.content)))
            .subscribe();
    }

    public cancelStatus() {
        this.contentsState.cancelStatus(this.content);
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

    public trackByEvent(_index: number, event: HistoryEventDto) {
        return event.eventId;
    }
}

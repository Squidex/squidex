/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { timer } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppsState, ConfirmClickDirective, ContentsState, ContentStatusComponent, CopyDirective, defined, DropdownMenuComponent, FormHintComponent, FromNowPipe, HistoryEventDto, HistoryService, LayoutComponent, ModalDirective, ModalModel, ModalPlacementDirective, SchemasState, switchSafe, TourStepDirective, TranslatePipe } from '@app/shared';
import { DueTimeSelectorComponent } from '../../shared/due-time-selector.component';
import { ContentEventComponent } from './content-event.component';
import { ContentPageComponent } from './content-page.component';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./content-history-page.component.scss'],
    templateUrl: './content-history-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ContentEventComponent,
        ContentStatusComponent,
        CopyDirective,
        DropdownMenuComponent,
        DueTimeSelectorComponent,
        FormHintComponent,
        FromNowPipe,
        LayoutComponent,
        ModalDirective,
        ModalPlacementDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class ContentHistoryPageComponent {
    private readonly dueTimeSelector = viewChild.required<DueTimeSelectorComponent>('dueTimeSelector');

    public readonly content = toSignal(this.contentsState.selectedContent.pipe(defined()));

    public readonly contentEvents = toSignal(
        this.contentsState.selectedContent.pipe(
            defined(),
            map(content => `schemas.${this.schemasState.schemaId}.contents.${content.id}`),
            switchSafe(channel => timer(0, 5000).pipe(map(() => channel))),
            switchSafe(channel => this.historyService.getHistory(this.appsState.appName, channel))),
        { initialValue: [] });

    public readonly dropdown = new ModalModel();
    public readonly dropdownNew = new ModalModel();

    public readonly settings = toSignal(this.appsState.selectedSettings, { initialValue: this.appsState.snapshot.selectedSettings });

    public readonly disableScheduler = computed(() => {
        return this.settings()?.hideScheduler === true;
    });

    constructor(
        private readonly appsState: AppsState,
        private readonly contentPage: ContentPageComponent,
        private readonly contentsState: ContentsState,
        private readonly historyService: HistoryService,
        private readonly schemasState: SchemasState,
    ) {
    }

    public changeStatus(status: string) {
        this.contentPage.checkPendingChangesBeforeChangingStatus().pipe(
                defined(),
                switchSafe(_ => this.dueTimeSelector().selectDueTime(status)),
                switchSafe(d => this.contentsState.changeManyStatus([this.content()!], status, d)))
            .subscribe();
    }

    public createDraft() {
        this.contentPage.checkPendingChangesBeforeChangingStatus().pipe(
                defined(),
                switchSafe(() => this.contentsState.createDraft(this.content()!)))
            .subscribe();
    }

    public cancelStatus() {
        this.contentsState.cancelStatus(this.content()!);
    }

    public delete() {
        this.contentsState.deleteMany([this.content()!]);
    }

    public deleteDraft() {
        this.contentsState.deleteDraft(this.content()!);
    }

    public loadVersion(event: HistoryEventDto) {
        this.contentPage.loadVersion(event.version, false);
    }

    public compareVersion(event: HistoryEventDto) {
        this.contentPage.loadVersion(event.version, true);
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, viewChild, inject } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
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
    private readonly appsState = inject(AppsState);
    private readonly contentPage = inject(ContentPageComponent);
    private readonly contentsState = inject(ContentsState);
    private readonly historyService = inject(HistoryService);
    private readonly schemasState = inject(SchemasState);

    private readonly dueTimeSelector = viewChild.required<DueTimeSelectorComponent>('dueTimeSelector');

    protected readonly content = computed(() => {
        return this.contentsState.selectedContent();
    });

    protected readonly contentEvents = toSignal(
        toObservable(this.contentsState.selectedContent).pipe(
            defined(),
            map(content => `schemas.${this.schemasState.schemaId}.contents.${content.id}`),
            switchSafe(channel => timer(0, 5000).pipe(map(() => channel))),
            switchSafe(channel => this.historyService.getHistory(this.appsState.appName, channel))),
        { initialValue: [] });

    protected readonly dropdown = new ModalModel();
    protected readonly dropdownNew = new ModalModel();

    protected readonly disableScheduler = computed(() => {
        return this.appsState.selectedSettings()?.hideScheduler === true;
    });

    protected changeStatus(status: string) {
        this.contentPage.checkPendingChangesBeforeChangingStatus().pipe(
                defined(),
                switchSafe(_ => this.dueTimeSelector().selectDueTime(status)),
                switchSafe(d => this.contentsState.changeManyStatus([this.content()!], status, d)))
            .subscribe();
    }

    protected createDraft() {
        this.contentPage.checkPendingChangesBeforeChangingStatus().pipe(
                defined(),
                switchSafe(() => this.contentsState.createDraft(this.content()!)))
            .subscribe();
    }

    protected cancelStatus() {
        this.contentsState.cancelStatus(this.content()!);
    }

    protected delete() {
        this.contentsState.deleteMany([this.content()!]);
    }

    protected deleteDraft() {
        this.contentsState.deleteDraft(this.content()!);
    }

    protected loadVersion(event: HistoryEventDto) {
        this.contentPage.loadVersion(event.version, false);
    }

    protected compareVersion(event: HistoryEventDto) {
        this.contentPage.loadVersion(event.version, true);
    }
}

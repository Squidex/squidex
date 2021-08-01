/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { AppsState, ContentDto, ContentsService, DateTime, DialogModel, getContentValue, LanguageDto, LanguagesState, LocalizerService, ResourceLoaderService } from '@app/shared';

declare const tui: any;

@Component({
    selector: 'sqx-calendar-page',
    styleUrls: ['./calendar-page.component.scss'],
    templateUrl: './calendar-page.component.html',
})
export class CalendarPageComponent implements AfterViewInit, OnDestroy {
    private calendar: any;
    private language: LanguageDto;

    @ViewChild('calendarContainer', { static: false })
    public calendarContainer: ElementRef;

    public content?: ContentDto;
    public contentDialog = new DialogModel();

    public isLoading: boolean;

    constructor(
        private readonly appsState: AppsState,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsService: ContentsService,
        private readonly resourceLoader: ResourceLoaderService,
        private readonly languagesState: LanguagesState,
        private readonly localizer: LocalizerService,
    ) {
    }

    public ngOnDestroy() {
        this.calendar?.destroy();
    }

    public ngOnInit() {
        this.language = this.languagesState.snapshot.languages.find(x => x.language.isMaster)!.language;
    }

    public ngAfterViewInit() {
        Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/tui-calendar/tui-calendar.min.css'),
            this.resourceLoader.loadLocalScript('dependencies/tui-calendar/tui-code-snippet.min.js'),
            this.resourceLoader.loadLocalScript('dependencies/tui-calendar/tui-calendar.min.js'),
        ]).then(() => {
            const Calendar = tui.Calendar;

            this.calendar = new Calendar(this.calendarContainer.nativeElement, {
                taskView: false,
                defaultView: 'month',
                isReadOnly: true,
            });

            this.calendar.on('clickSchedule', (event: any) => {
                this.content = event.schedule.raw;
                this.contentDialog.show();

                this.changeDetector.detectChanges();
            });

            this.load();
        });
    }

    public goPrev() {
        this.calendar?.prev();

        this.load();
    }

    public goNext() {
        this.calendar?.next();

        this.load();
    }

    private load() {
        if (!this.calendar || this.isLoading) {
            return;
        }

        this.isLoading = true;

        const scheduledFrom = new DateTime(this.calendar.getDateRangeStart().toDate()).toISOString();
        const scheduledTo = new DateTime(this.calendar.getDateRangeEnd().toDate()).toISOString();

        this.contentsService.getAllContents(this.appsState.appName, {
            scheduledFrom,
            scheduledTo,
        }).subscribe({
            next: contents => {
                this.calendar.clear();
                this.calendar.createSchedules(contents.items.map(x => ({
                    id: x.id,
                    borderColor: x.scheduleJob!.color,
                    color: x.scheduleJob?.color,
                    calendarId: '1',
                    category: 'time',
                    end: x.scheduleJob?.dueTime.toISOString(),
                    raw: x,
                    start: x.scheduleJob?.dueTime.toISOString(),
                    state: 'free',
                    title: `[${x.schemaDisplayName}] ${this.createContentName(x)}`,
                })));
            },
            complete: () => {
                this.isLoading = false;
            },
        });
    }

    public createContentName(content: ContentDto) {
        const name =
            content.referenceFields
                .map(f => getContentValue(content, this.language, f, false))
                .map(v => v.formatted)
                .filter(v => !!v)
                .join(', ')
            || this.localizer.getOrKey('common.noValue');

        return name;
    }
}

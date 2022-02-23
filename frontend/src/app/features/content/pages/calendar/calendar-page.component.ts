/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, HostListener, OnDestroy, ViewChild } from '@angular/core';
import { AppsState, ContentDto, ContentsService, DateTime, DialogModel, getContentValue, LanguageDto, LanguagesState, LocalizerService, ResourceLoaderService } from '@app/shared';

declare const tui: any;

type ViewMode = 'day' | 'week' | 'month';

@Component({
    selector: 'sqx-calendar-page',
    styleUrls: ['./calendar-page.component.scss'],
    templateUrl: './calendar-page.component.html',
})
export class CalendarPageComponent implements AfterViewInit, OnDestroy {
    private calendar: any;
    private language!: LanguageDto;

    @ViewChild('calendarContainer', { static: false })
    public calendarContainer!: ElementRef;

    public view: ViewMode = 'month';

    public content?: ContentDto;
    public contentDialog = new DialogModel();

    public title = '';

    public isLoading = false;

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
                defaultView: 'month',
                isReadOnly: true,
                scheduleView: ['time'],
                taskView: false,
                ...getLocalizationSettings(),
            });

            this.calendar.on('clickSchedule', (event: any) => {
                this.content = event.schedule.raw;
                this.contentDialog.show();

                this.changeDetector.detectChanges();
            });

            this.calendar.on('clickDayname', (event: any) => {
                if (this.calendar.getViewName() === 'week') {
                    this.calendar.setDate(new Date(event.date));

                    this.changeView('day');
                }
            });

            this.load();
        });
    }

    @HostListener('click', ['$event'])
    public onClick(event: MouseEvent) {
        const target = event.target as HTMLElement;

        if (target.classList.contains('tui-full-calendar-weekday-grid-date')) {
            const monthStart: Date = this.calendar.getDateRangeStart().toDate();

            const dayOfMonth = parseInt(target.innerText, 10);
            const dateOfMonth = new Date(monthStart.getFullYear(), monthStart.getMonth(), dayOfMonth);

            this.calendar.setDate(dateOfMonth);

            this.changeView('day');
        }
    }

    public changeView(view: ViewMode) {
        this.view = view;

        this.calendar?.changeView(view);

        this.load();
    }

    public goPrev() {
        this.calendar?.prev();

        this.load();
    }

    public goNext() {
        this.calendar?.next();

        this.load();
    }

    public cancelStatus() {
        this.contentsService.cancelStatus(this.appsState.appName, this.content!, this.content!.version)
            .subscribe(content => {
                this.calendar?.deleteSchedule(content.id, '1');

                this.contentDialog.hide();
                this.content = undefined;
            });
    }

    private load() {
        if (!this.calendar) {
            return;
        }

        const scheduledFrom = new DateTime(this.calendar.getDateRangeStart().toDate());
        const scheduledTo = new DateTime(this.calendar.getDateRangeEnd().toDate());

        this.updateRange(scheduledFrom, scheduledTo);

        if (this.isLoading) {
            return;
        }

        this.isLoading = true;

        this.contentsService.getAllContents(this.appsState.appName, {
            scheduledFrom: scheduledFrom.toISOString(),
            scheduledTo: scheduledTo.toISOString(),
        }).subscribe({
            next: contents => {
                this.calendar.clear();
                this.calendar.createSchedules(contents.items.map(x => ({
                    id: x.id,
                    bgColor: '#fff',
                    borderColor: x.scheduleJob!.color,
                    color: 'x.scheduleJob?.color',
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

    private updateRange(from: DateTime, to: DateTime) {
        switch (this.view) {
            case 'month': {
                this.title = from.toStringFormat('LLLL yyyy');
                break;
            }
            case 'day': {
                this.title = from.toStringFormat('PPPP');
                break;
            }
            case 'week': {
                this.title = `${from.toStringFormat('PP')} - ${to.toStringFormat('PP')}`;
                break;
            }
        }
    }

    public createContentName(content: ContentDto) {
        const name =
            content.referenceFields
                .map(f => getContentValue(content, this.language, f, false))
                .map(v => v.formatted)
                .defined()
                .join(', ')
            || this.localizer.getOrKey('common.noValue');

        return name;
    }
}

let localizedValues: any;

function getLocalizationSettings() {
    if (!localizedValues) {
        localizedValues = {
            month: {
                daynames: [],
            },
            week: {
                daynames: [],
            },
            template: {
                timegridDisplayPrimaryTime: (time: any) => {
                    return new DateTime(new Date(2020, 1, 1, time.hour, time.minutes, 0)).toStringFormat('p');
                },
                timegridCurrentTime: (timezone: any) => {
                    const templates = [];

                    if (timezone.dateDifference) {
                        templates.push(`[${timezone.dateDifferenceSign}${timezone.dateDifference}]<br>`);
                    }

                    templates.push(new DateTime(timezone.hourmarker.toDate()).toStringFormat('p'));

                    return templates.join('');
                },
            },
        };

        for (let i = 1; i <= 7; i++) {
            const weekDay = new DateTime(new Date(2020, 10, i, 12, 0, 0));

            localizedValues.month.daynames.push(weekDay.toStringFormat('EEE'));
            localizedValues.week.daynames.push(weekDay.toStringFormat('EEE'));
        }
    }

    return localizedValues;
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { afterNextRender, Component, DestroyRef, ElementRef, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppLanguageDto, AppsState, ConfirmClickDirective, ContentDto, ContentsService, ContentStatusComponent, CopyDirective, DateTime, DialogModel, FormRowComponent, FullDateTimePipe, getContentValue, LanguagesState, LayoutComponent, LocalizerService, ModalDialogComponent, ModalDirective, ResourceLoaderService, TitleComponent, TooltipDirective, TranslatePipe, UserNameRefPipe, UserPictureRefPipe } from '@app/shared';

declare const tui: any;

type ViewMode = 'day' | 'week' | 'month';

@Component({
    selector: 'sqx-calendar-page',
    styleUrls: ['./calendar-page.component.scss'],
    templateUrl: './calendar-page.component.html',
    imports: [
        ConfirmClickDirective,
        ContentStatusComponent,
        CopyDirective,
        FormRowComponent,
        FullDateTimePipe,
        LayoutComponent,
        ModalDialogComponent,
        ModalDirective,
        RouterLink,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
    host: {
        '(click)': 'onClick($event)',
    },
})
export class CalendarPageComponent {
    private readonly appsState = inject(AppsState);
    private readonly contentsService = inject(ContentsService);
    private readonly destroyRef = inject(DestroyRef);
    private readonly languagesState = inject(LanguagesState);
    private readonly language: AppLanguageDto = this.languagesState.snapshot.languages.find(x => x.language.isMaster)!.language;
    private readonly localizer = inject(LocalizerService);
    private readonly resourceLoader = inject(ResourceLoaderService);
    private calendar: any;

    protected readonly calendarContainer = viewChild.required<ElementRef>('calendarContainer');

    protected readonly view = signal<ViewMode>('month');

    protected readonly contentSelected = signal<ContentDto | undefined>(undefined);

    protected readonly title = signal('');

    protected readonly isLoading = signal(false);

    protected readonly contentDialog = new DialogModel();

    constructor() {
        afterNextRender(() => {
            this.initCalendar();
        });

        this.destroyRef.onDestroy(() => {
            this.calendar?.destroy();
        });
    }

    private async initCalendar() {
        await Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/tui-calendar/tui-calendar.min.css'),
            this.resourceLoader.loadLocalScript('dependencies/tui-calendar/tui-code-snippet.min.js'),
            this.resourceLoader.loadLocalScript('dependencies/tui-calendar/tui-calendar.min.js'),
        ]);

        this.calendar?.destroy();
        this.calendar = new tui.Calendar(this.calendarContainer().nativeElement, {
            defaultView: 'month',
            isReadOnly: true,
            isLoading: false,
            scheduleView: ['time'],
            taskView: false,
            ...getLocalizationSettings(),
        });

        this.calendar.on('clickSchedule', (event: any) => {
            this.contentSelected.set(event.schedule.raw);
            this.contentDialog.show();
        });

        this.calendar.on('clickDayname', (event: any) => {
            if (this.calendar.getViewName() === 'week') {
                this.calendar.setDate(new Date(event.date));

                this.changeView('day');
            }
        });

        this.load();
    }

    protected onClick(event: MouseEvent) {
        const target = event.target as HTMLElement;

        if (target.classList.contains('tui-full-calendar-weekday-grid-date')) {
            const monthStart: Date = this.calendar.getDateRangeStart().toDate();

            const dayOfMonth = parseInt(target.innerText, 10);
            const dateOfMonth = new Date(monthStart.getFullYear(), monthStart.getMonth(), dayOfMonth);

            this.calendar.setDate(dateOfMonth);

            this.changeView('day');
        }
    }

    protected changeView(view: ViewMode) {
        this.view.set(view);

        this.calendar?.changeView(view);

        this.load();
    }

    protected goPrev() {
        this.calendar?.prev();

        this.load();
    }

    protected goNext() {
        this.calendar?.next();

        this.load();
    }

    protected cancelStatus() {
        const contentSelected = this.contentSelected();
        if (!contentSelected) {
            return;
        }

        this.contentsService.cancelStatus(this.appsState.appName, contentSelected, contentSelected.version)
            .subscribe(content => {
                this.calendar?.deleteSchedule(content.id, '1');

                this.contentDialog.hide();
                this.contentSelected.set(undefined);
            });
    }

    private load() {
        if (!this.calendar) {
            return;
        }

        const scheduledFrom = new DateTime(this.calendar.getDateRangeStart().toDate());
        const scheduledTo = new DateTime(this.calendar.getDateRangeEnd().toDate());

        this.updateRange(scheduledFrom, scheduledTo);

        if (this.isLoading()) {
            return;
        }

        this.isLoading.set(true);

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
                this.isLoading.set(false);
            },
        });
    }

    private updateRange(from: DateTime, to: DateTime) {
        switch (this.view()) {
            case 'month': {
                this.title.set(from.toStringFormat('LLLL yyyy'));
                break;
            }
            case 'day': {
                this.title.set(from.toStringFormat('PPPP'));
                break;
            }
            case 'week': {
                this.title.set(`${from.toStringFormat('PP')} - ${to.toStringFormat('PP')}`);
                break;
            }
        }
    }

    protected createContentName(content: ContentDto) {
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

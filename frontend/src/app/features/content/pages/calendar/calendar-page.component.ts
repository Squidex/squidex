/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { afterNextRender, ChangeDetectionStrategy, Component, DestroyRef, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AppLanguageDto, AppsState, ConfirmClickDirective, ContentDto, ContentsService, ContentStatusComponent, CopyDirective, DateTime, DialogModel, FormRowComponent, FullDateTimePipe, getContentValue, LanguagesState, LayoutComponent, LocalizerService, ModalDialogComponent, ModalDirective, ResourceLoaderService, TitleComponent, TooltipDirective, TranslatePipe, UserNameRefPipe, UserPictureRefPipe } from '@app/shared';

declare const tui: any;

type ViewMode = 'day' | 'week' | 'month';

@Component({
    selector: 'sqx-calendar-page',
    styleUrls: ['./calendar-page.component.scss'],
    templateUrl: './calendar-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ContentStatusComponent,
        CopyDirective,
        FormRowComponent,
        FormsModule,
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
    private readonly calendarContainer = viewChild.required<ElementRef>('calendarContainer');
    private readonly language: AppLanguageDto;
    private calendar: any;

    public readonly view = signal<ViewMode>('month');

    public readonly contentSelected = signal<ContentDto | undefined>(undefined);
    public readonly contentDialog = new DialogModel();

    public readonly title = signal('');

    public readonly isLoading = signal(false);

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly resourceLoader: ResourceLoaderService,
        private readonly languagesState: LanguagesState,
        private readonly localizer: LocalizerService,
    ) {
        this.language = this.languagesState.snapshot.languages.find(x => x.language.isMaster)!.language;

        afterNextRender(() => {
            this.initCalendar();
        });

        inject(DestroyRef).onDestroy(() => {
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
        this.view.set(view);

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

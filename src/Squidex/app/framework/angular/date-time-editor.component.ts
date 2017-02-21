/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import * as moment from 'moment';

let Pikaday = require('pikaday/pikaday');


/* tslint:disable:no-empty */

const NOOP = () => { };

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => DateTimeEditorComponent),
    multi: true
};

const TIMEZONES: any[] = [
    { label: 'UTC-13:00', value: -780 },
    { label: 'UTC-12:00', value: -720 },
    { label: 'UTC-11:00', value: -660 },
    { label: 'UTC-10:00', value: -600 },
    { label: 'UTC-09:30', value: -570 },
    { label: 'UTC-09:00', value: -540 },
    { label: 'UTC-08:00', value: -480 },
    { label: 'UTC-07:00', value: -420 },
    { label: 'UTC-06:00', value: -360 },
    { label: 'UTC-05:00', value: -300 },
    { label: 'UTC-04:30', value: -270 },
    { label: 'UTC-04:00', value: -240 },
    { label: 'UTC-03:30', value: -210 },
    { label: 'UTC-03:00', value: -180 },
    { label: 'UTC-02:00', value: -120 },
    { label: 'UTC-01:00', value: -60 },
    { label: 'UTC+00:00', value: 0 },
    { label: 'UTC+01:00', value: 60 },
    { label: 'UTC+02:00', value: 120 },
    { label: 'UTC+03:00', value: 180 },
    { label: 'UTC+03:30', value: 210 },
    { label: 'UTC+04:00', value: 240 },
    { label: 'UTC+04:30', value: 270 },
    { label: 'UTC+05:00', value: 300 },
    { label: 'UTC+05:30', value: 330 },
    { label: 'UTC+05:45', value: 345 },
    { label: 'UTC+06:00', value: 360 },
    { label: 'UTC+06:30', value: 390 },
    { label: 'UTC+07:00', value: 420 },
    { label: 'UTC+08:00', value: 480 },
    { label: 'UTC+08:45', value: 425 },
    { label: 'UTC+09:00', value: 540 },
    { label: 'UTC+09:30', value: 570 },
    { label: 'UTC+10:00', value: 600 },
    { label: 'UTC+10:30', value: 630 },
    { label: 'UTC+11:00', value: 660 },
    { label: 'UTC+11:30', value: 690 },
    { label: 'UTC+12:00', value: 720 },
    { label: 'UTC+12:45', value: 765 },
    { label: 'UTC+13:00', value: 780 },
    { label: 'UTC+14:00', value: 840 }
];

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class DateTimeEditorComponent implements ControlValueAccessor, OnInit, AfterViewInit {
    private picker: any;
    private time: any;
    private date: any;
    private offset: number;
    private suppressEvents = false;
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    public get showTime() {
        return this.mode === 'DateTime' || this.mode === 'DateTimeWithTimezone';
    }

    public get showTimezone() {
        return this.mode === 'DateWithTimezone' || this.mode === 'DateTimeWithTimezone';
    }

    public timezones = TIMEZONES;

    public timeControl =
        new FormControl();

    public timeZoneControl =
        new FormControl();

    public isDisabled = false;

    @Input()
    public mode: string;

    @ViewChild('dateInput')
    public dateInput: ElementRef;

    public ngOnInit() {
        this.timeControl.valueChanges.subscribe(value => {
            const time = moment(value, 'HH:mm:ss');

            this.time = moment();
            this.time.hours(time.hours()).minutes(time.minutes()).seconds(time.seconds());

            this.updateValue();
        });

        this.timeZoneControl.valueChanges.subscribe(value => {
            this.offset = value;

            this.updateValue();
            this.touched();
        });
    }

    public writeValue(value: any) {
        const parsed = (moment.parseZone(value) || moment());

        this.time = moment(parsed);
        this.date = moment(parsed);

        this.offset = parsed.utcOffset();

        this.updateControls();
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (isDisabled) {
            this.timeControl.disable();
            this.timeZoneControl.disable();
        } else {
            this.timeControl.enable();
            this.timeZoneControl.enable();
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public ngAfterViewInit() {
        this.picker = new Pikaday({
            field: this.dateInput.nativeElement,
            format: 'YYYY-MM-DD',
            onSelect: () => {
                if (this.suppressEvents) {
                    return;
                }

                const date = this.picker.getMoment();

                this.date.years(date.years()).months(date.months()).dates(date.dates());

                this.updateValue();
                this.touched();
            }
        });

        this.updateControls();
    }

    public touched() {
        this.touchedCallback();
    }

    private updateValue() {
        let result = this.date.format('YYYY-MM-DD');

        if (this.showTime) {
            result += 'T';
            result += this.time.format('HH:mm:ss');
        }

        if (this.showTimezone) {
            result += moment().utcOffset(this.offset).format('Z');
        } else if (this.showTime) {
            result += 'Z';
        }

        this.changeCallback(result);
    }

    private updateControls() {
        if (!this.date) {
            return;
        }

        this.suppressEvents = true;

        this.timeControl.setValue(this.time.format('HH:mm'), { emitEvent: false });
        this.timeZoneControl.setValue(this.offset, { emitEvent: false });

        if (this.picker) {
            this.picker.setMoment(this.date);
        }

        this.suppressEvents = false;
    }
}
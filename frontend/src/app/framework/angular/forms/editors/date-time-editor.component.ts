/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, forwardRef, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR, UntypedFormControl } from '@angular/forms';
import * as Pikaday from 'pikaday/pikaday';
import { DateHelper, DateTime, StatefulControlComponent, UIOptions } from '@app/framework/internal';
import { FocusComponent } from './../forms-helper';

declare module 'pikaday/pikaday';

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimeEditorComponent), multi: true,
};

const NO_EMIT = { emitEvent: false };

interface State {
    // True when the editor is in local mode.
    isLocal: boolean;
}

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [
        SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DateTimeEditorComponent extends StatefulControlComponent<State, string | null> implements OnInit, AfterViewInit, FocusComponent {
    private readonly hideDateButtonsSettings: boolean;
    private readonly hideDateTimeModeButtonSetting: boolean;
    private picker: any;
    private dateTime?: DateTime | null;
    private suppressEvents = false;

    @Output()
    public blur = new EventEmitter();

    @Input()
    public mode: 'DateTime' | 'Date' = 'Date';

    @Input()
    public enforceTime?: boolean | null;

    @Input()
    public hideClear?: boolean | null;

    @Input()
    public hideDateButtons?: boolean | null;

    @Input()
    public hideDateTimeModeButton?: boolean | null;

    @Input()
    public size: 'Normal' | 'Compact' | 'Mini' = 'Normal';

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('dateInput', { static: false })
    public dateInput!: ElementRef<HTMLInputElement>;

    public readonly timeControl = new UntypedFormControl();
    public readonly dateControl = new UntypedFormControl();

    public get shouldShowDateButtons() {
        return !this.hideDateButtonsSettings && !this.hideDateButtons;
    }

    public get shouldShowDateTimeModeButton() {
        return !this.hideDateTimeModeButtonSetting && !this.hideDateTimeModeButton;
    }

    public get isDateTimeMode() {
        return this.mode === 'DateTime';
    }

    public get hasValue() {
        return !!this.dateTime;
    }

    constructor(changeDetector: ChangeDetectorRef, uiOptions: UIOptions) {
        super(changeDetector, {
            isLocal: true,
        });

        this.hideDateButtonsSettings = !!uiOptions.get('hideDateButtons');
        this.hideDateTimeModeButtonSetting = !!uiOptions.get('hideDateTimeModeButton');
    }

    public ngOnInit() {
        this.own(
            this.timeControl.valueChanges.subscribe(() => {
                this.dateTime = this.getValue();

                this.callChangeFormatted();
            }));

        this.own(
            this.dateControl.valueChanges.subscribe(() => {
                this.dateTime = this.getValue();

                this.callChangeFormatted();
            }));
    }

    public writeValue(obj: any) {
        try {
            this.dateTime = DateTime.parseISO(obj, false);
        } catch (ex) {
            this.dateTime = null;
        }

        this.updateControls();
    }

    public callTouched() {
        this.blur.next(true);

        super.callTouched();
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.dateControl.disable(NO_EMIT);
            this.timeControl.disable(NO_EMIT);
        } else {
            this.dateControl.enable(NO_EMIT);
            this.timeControl.enable(NO_EMIT);
        }
    }

    public focus() {
        this.dateInput.nativeElement.focus();
    }

    public ngAfterViewInit() {
        const i18n = getLocalizationSettings();

        this.picker = new Pikaday({
            field: this.dateInput.nativeElement,
            i18n,
            format: 'YYYY-MM-DD',
            onSelect: () => {
                if (this.suppressEvents) {
                    return;
                }

                this.dateControl.setValue(this.picker.toString('YYYY-MM-DD'));

                this.callTouched();
            },
        });

        this.updateControls();
    }

    public writeToday() {
        this.dateTime = new DateTime(DateHelper.getLocalDate(DateTime.today().raw));

        this.updateControls();
        this.callChangeFormatted();
        this.callTouched();

        return false;
    }

    public writeNow() {
        this.dateTime = DateTime.now();

        this.updateControls();
        this.callChangeFormatted();
        this.callTouched();

        return false;
    }

    public reset() {
        this.dateTime = null;

        this.updateControls();
        this.callChangeFormatted();
        this.callTouched();

        return false;
    }

    private callChangeFormatted() {
        this.callChange(this.dateTime?.toISOString());
    }

    private getValue(): DateTime | null {
        if (!this.dateControl.value) {
            return null;
        }

        if (this.isDateTimeMode && this.timeControl.value) {
            const combined = `${this.dateControl.value}T${this.timeControl.value}`;

            return DateTime.tryParseISO(combined, !this.snapshot.isLocal);
        }

        return DateTime.tryParseISO(this.dateControl.value);
    }

    private updateControls() {
        this.suppressEvents = true;

        if (this.dateTime && this.isDateTimeMode) {
            if (this.snapshot.isLocal) {
                this.timeControl.setValue(this.dateTime.toStringFormat('HH:mm:ss'), NO_EMIT);
            } else {
                this.timeControl.setValue(this.dateTime.toStringFormatUTC('HH:mm:ss'), NO_EMIT);
            }
        } else {
            this.timeControl.setValue(null, NO_EMIT);
        }

        if (this.dateTime && this.picker) {
            let dateString: string;

            if (this.isDateTimeMode && this.snapshot.isLocal) {
                dateString = this.dateTime.toStringFormat('yyyy-MM-dd');

                this.picker.setDate(DateHelper.getLocalDate(this.dateTime.raw), true);
            } else {
                dateString = this.dateTime.toStringFormatUTC('yyyy-MM-dd');

                this.picker.setDate(DateHelper.getUTCDate(this.dateTime.raw), true);
            }

            this.dateControl.setValue(dateString, NO_EMIT);
        } else {
            this.dateControl.setValue(null, NO_EMIT);
        }

        this.suppressEvents = false;
    }

    public setLocalMode(isLocal: boolean) {
        this.next({ isLocal });

        this.updateControls();
    }

    public setSize(size: DOMRect) {
        if (size.width < 300) {
            this.size = 'Mini';
        } else if (size.width < 350) {
            this.size = 'Compact';
        } else {
            this.size = 'Normal';
        }
    }
}

let localizedValues: any;

function getLocalizationSettings() {
    if (!localizedValues) {
        localizedValues = {
            months: [],
            weekdays: [],
            weekdaysShort: [],
        };

        for (let i = 0; i < 12; i++) {
            const firstOfMonth = new DateTime(new Date(2020, i, 1, 12, 0, 0));

            localizedValues.months.push(firstOfMonth.toStringFormat('LLLL'));
        }

        for (let i = 1; i <= 7; i++) {
            const weekDay = new DateTime(new Date(2020, 10, i, 12, 0, 0));

            localizedValues.weekdays.push(weekDay.toStringFormat('EEEE'));
            localizedValues.weekdaysShort.push(weekDay.toStringFormat('EEE'));
        }
    }

    return localizedValues;
}

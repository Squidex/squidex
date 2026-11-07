/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, forwardRef, inject, input, linkedSignal, output, signal, viewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { DateHelper, DateTime, SignalControlComponent, UIOptions } from '@app/framework/internal';
import { TooltipDirective } from '../../modals/tooltip.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ResizedDirective } from '../../resized.directive';
import { FocusComponent } from '../forms-helper';
import { ForwardFormClassesDirective } from '../forward-form-classes.directive';

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimeEditorComponent), multi: true,
};

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [
        SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        ForwardFormClassesDirective,
        ReactiveFormsModule,
        ResizedDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DateTimeEditorComponent extends SignalControlComponent<string | null> implements FocusComponent {
    private readonly hideDateButtonsSettings = !!inject(UIOptions).value.hideDateButtons;
    private readonly hideDateTimeModeButtonSetting = !!inject(UIOptions).value.hideDateTimeModeButton;
    private readonly dateInput = viewChild.required<ElementRef<HTMLInputElement>>('dateInput');

    /** Emits when the editor loses the focus. */
    public readonly editorBlur = output<boolean>();

    /** Edits only the date or the date and time. */
    public readonly mode = input<'DateTime' | 'Date'>('Date');

    /** The ID of the inner input. */
    public readonly id = input('');

    /** Hides the button to clear the value. */
    public readonly hideClear = input(false, { transform: booleanAttribute });

    /** Hides the buttons to set the value to today or now. */
    public readonly hideDateButtons = input(false, { transform: booleanAttribute });

    /** Hides the button to switch between local time and UTC. */
    public readonly hideDateTimeModeButton = input(false, { transform: booleanAttribute });

    /** The initial size of the editor. It is adjusted to the available width. */
    public readonly size = input<'Normal' | 'Compact' | 'Mini'>('Normal');

    public readonly sizeActual = linkedSignal(() => this.size());

    public readonly isLocal = signal(true);

    public readonly isDateTimeMode = computed(() => {
        return this.mode() === 'DateTime';
    });

    public readonly shouldShowDateButtons = computed(() => {
        return !this.hideDateButtonsSettings && !this.hideDateButtons();
    });

    public readonly shouldShowDateTimeModeButton = computed(() => {
        return !this.hideDateTimeModeButtonSetting && !this.hideDateTimeModeButton();
    });

    public readonly value = signal<DateTime | null | undefined>(undefined);

    public readonly valueInput = computed(() => {
        const value = this.value();
        if (!value) {
            return null;
        }

        const format = this.isDateTimeMode() ? 'yyyy-MM-dd\'T\'HH:mm:ss' : 'yyyy-MM-dd';

        return this.isLocal() ? value.toStringFormat(format) : value.toStringFormatUTC(format);
    });

    public callTouched() {
        this.editorBlur.emit(true);

        super.callTouched();
    }

    public focus() {
        this.dateInput().nativeElement.focus();
    }

    public writeValue(obj: any) {
        let value: DateTime | null;
        try {
            value = DateTime.parseISO(obj, false);
        } catch {
            value = null;
        }

        this.value.set(value);
    }

    public writeToday() {
        this.update(new DateTime(DateHelper.getLocalDate(DateTime.today().raw)));
        return false;
    }

    public writeNow() {
        this.update(DateTime.now());
        return false;
    }

    public reset() {
        this.update(null);
        return false;
    }

    public setLocalMode(isLocal: boolean) {
        this.isLocal.set(isLocal);
        this.update(this.value());
        return false;
    }

    public updateValue(source: string) {
        this.update(DateTime.tryParseISO(source, !this.isLocal()));
        return false;
    }

    private update(value: DateTime | null | undefined) {
        this.value.set(value);

        this.callChange(value?.toISOString());
        this.callTouched();
    }

    public setSize(size: DOMRect) {
        if (size.width < 300) {
            this.sizeActual.set('Mini');
        } else if (size.width < 350) {
            this.sizeActual.set('Compact');
        } else {
            this.sizeActual.set('Normal');
        }
    }
}
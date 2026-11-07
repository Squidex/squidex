/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, inject, input, linkedSignal, model, output, signal, viewChild } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { DateHelper, DateTime, UIOptions } from '@app/framework/internal';
import { TooltipDirective } from '../../modals/tooltip.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ResizedDirective } from '../../resized.directive';
import { FocusComponent } from '../forms-helper';

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ResizedDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DateTimeEditorComponent implements FormValueControl<string | null>, FocusComponent {
    private readonly uiOptions = inject(UIOptions);
    private readonly hideDateTimeModeButtonSetting = !!this.uiOptions.value.hideDateTimeModeButton;
    private readonly hideDateButtonsSettings = !!this.uiOptions.value.hideDateButtons;

    /** The date as ISO string. */
    public readonly value = model<string | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** Indicates whether the value is invalid, provided by the form. */
    public readonly invalid = input(false, { transform: booleanAttribute });

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

    protected readonly dateInput = viewChild.required<ElementRef<HTMLInputElement>>('dateInput');

    protected readonly sizeActual = linkedSignal(() => this.size());

    protected readonly isLocal = signal(true);

    protected readonly isDateTimeMode = computed(() => {
        return this.mode() === 'DateTime';
    });

    protected readonly shouldShowDateButtons = computed(() => {
        return !this.hideDateButtonsSettings && !this.hideDateButtons();
    });

    protected readonly shouldShowDateTimeModeButton = computed(() => {
        return !this.hideDateTimeModeButtonSetting && !this.hideDateTimeModeButton();
    });

    protected readonly valueParsed = computed(() => {
        try {
            return DateTime.parseISO(this.value()!, false);
        } catch {
            return null;
        }
    });

    protected readonly valueInput = computed(() => {
        const value = this.valueParsed();
        if (!value) {
            return null;
        }

        const format = this.isDateTimeMode() ? 'yyyy-MM-dd\'T\'HH:mm:ss' : 'yyyy-MM-dd';

        return this.isLocal() ? value.toStringFormat(format) : value.toStringFormatUTC(format);
    });

    public focus() {
        this.dateInput().nativeElement.focus();
    }

    protected markTouched() {
        this.editorBlur.emit(true);
        this.touched.set(true);
    }

    protected writeToday() {
        this.update(new DateTime(DateHelper.getLocalDate(DateTime.today().raw)));
        return false;
    }

    protected writeNow() {
        this.update(DateTime.now());
        return false;
    }

    protected reset() {
        this.update(null);
        return false;
    }

    protected setLocalMode(isLocal: boolean) {
        this.isLocal.set(isLocal);
        this.markTouched();
        return false;
    }

    protected updateValue(source: string) {
        this.update(DateTime.tryParseISO(source, !this.isLocal()));
        return false;
    }

    private update(value: DateTime | null) {
        this.value.set(value?.toISOString() ?? null);
        this.markTouched();
    }

    protected setSize(size: DOMRect) {
        if (size.width < 300) {
            this.sizeActual.set('Mini');
        } else if (size.width < 350) {
            this.sizeActual.set('Compact');
        } else {
            this.sizeActual.set('Normal');
        }
    }
}

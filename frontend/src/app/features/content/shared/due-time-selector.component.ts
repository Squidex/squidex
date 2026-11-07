/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { form, FormField } from '@angular/forms/signals';
import { Observable, of, Subject } from 'rxjs';
import { DateTime, DateTimeEditorComponent, DialogModel, FocusOnInitDirective, ModalDialogComponent, ModalDirective, TooltipDirective, TranslatePipe } from '@app/shared';

const OPTION_IMMEDIATELY = 'Immediately';

@Component({
    selector: 'sqx-due-time-selector',
    styleUrls: ['./due-time-selector.component.scss'],
    templateUrl: './due-time-selector.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DateTimeEditorComponent,
        FocusOnInitDirective,
        FormField,
        ModalDialogComponent,
        ModalDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DueTimeSelectorComponent {
    private dueTimeResult?: Subject<DateTime | undefined>;

    /** Skips the dialog and changes the status immediately. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    protected readonly dueTime = signal<string | null>('');

    protected readonly dueTimeAction = signal<string | null>('');

    protected readonly dueTimeDialog = new DialogModel();

    protected readonly dueTimeMode = form(signal(OPTION_IMMEDIATELY));

    public selectDueTime(action: string): Observable<DateTime | undefined> {
        if (this.disabled()) {
            return of(undefined);
        }

        this.dueTimeAction.set(action);
        this.dueTimeResult = new Subject<DateTime | undefined>();
        this.dueTimeDialog.show();

        return this.dueTimeResult;
    }

    protected confirmStatusChange() {
        const result = this.dueTimeMode().value() === OPTION_IMMEDIATELY ? null : this.dueTime();

        this.dueTimeResult?.next(result ? DateTime.parseISO(result) : undefined);
        this.dueTimeResult?.complete();

        this.cancelStatusChange();
    }

    protected cancelStatusChange() {
        this.dueTimeMode().value.set(OPTION_IMMEDIATELY);
        this.dueTimeDialog.hide();
        this.dueTimeResult = null!;
        this.dueTime.set(null);
    }
}

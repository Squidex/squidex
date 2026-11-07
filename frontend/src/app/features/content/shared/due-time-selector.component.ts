/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
        FormsModule,
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

    public readonly dueTimeDialog = new DialogModel();
    public readonly dueTime = signal<string | null>('');
    public readonly dueTimeAction = signal<string | null>('');
    public readonly dueTimeMode = signal(OPTION_IMMEDIATELY);

    public selectDueTime(action: string): Observable<DateTime | undefined> {
        if (this.disabled()) {
            return of(undefined);
        }

        this.dueTimeAction.set(action);
        this.dueTimeResult = new Subject<DateTime | undefined>();
        this.dueTimeDialog.show();

        return this.dueTimeResult;
    }

    public confirmStatusChange() {
        const result = this.dueTimeMode() === OPTION_IMMEDIATELY ? null : this.dueTime();

        this.dueTimeResult?.next(result ? DateTime.parseISO(result) : undefined);
        this.dueTimeResult?.complete();

        this.cancelStatusChange();
    }

    public cancelStatusChange() {
        this.dueTimeMode.set(OPTION_IMMEDIATELY);
        this.dueTimeDialog.hide();
        this.dueTimeResult = null!;
        this.dueTime.set(null);
    }
}

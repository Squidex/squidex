/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, of, Subject } from 'rxjs';
import { DateTime, DateTimeEditorComponent, DialogModel, FocusOnInitDirective, ModalDialogComponent, ModalDirective, TooltipDirective, TranslatePipe } from '@app/shared';

const OPTION_IMMEDIATELY = 'Immediately';

@Component({
    standalone: true,
    selector: 'sqx-due-time-selector',
    styleUrls: ['./due-time-selector.component.scss'],
    templateUrl: './due-time-selector.component.html',
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

    @Input({ transform: booleanAttribute })
    public disabled?: boolean | null;

    public dueTimeDialog = new DialogModel();
    public dueTime: string | null = '';
    public dueTimeAction: string | null = '';
    public dueTimeMode = OPTION_IMMEDIATELY;

    public selectDueTime(action: string): Observable<DateTime | undefined> {
        if (this.disabled) {
            return of(undefined);
        }

        this.dueTimeAction = action;
        this.dueTimeResult = new Subject<DateTime | undefined>();
        this.dueTimeDialog.show();

        return this.dueTimeResult;
    }

    public confirmStatusChange() {
        const result = this.dueTimeMode === OPTION_IMMEDIATELY ? null : this.dueTime;

        this.dueTimeResult?.next(result ? DateTime.parseISO(result) : undefined);
        this.dueTimeResult?.complete();

        this.cancelStatusChange();
    }

    public cancelStatusChange() {
        this.dueTimeMode = OPTION_IMMEDIATELY;
        this.dueTimeDialog.hide();
        this.dueTimeResult = null!;
        this.dueTime = null;
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { Observable, of, Subject } from 'rxjs';
import { DialogModel } from '@app/shared';

const OPTION_IMMEDIATELY = 'Immediately';

@Component({
    selector: 'sqx-due-time-selector',
    styleUrls: ['./due-time-selector.component.scss'],
    templateUrl: './due-time-selector.component.html',
})
export class DueTimeSelectorComponent {
    private dueTimeResult?: Subject<string | null>;

    @Input()
    public disabled?: boolean | null;

    public dueTimeDialog = new DialogModel();
    public dueTime: string | null = '';
    public dueTimeAction: string | null = '';
    public dueTimeMode = OPTION_IMMEDIATELY;

    public selectDueTime(action: string): Observable<string | null> {
        if (this.disabled) {
            return of(null);
        }

        this.dueTimeAction = action;
        this.dueTimeResult = new Subject<string | null>();
        this.dueTimeDialog.show();

        return this.dueTimeResult;
    }

    public confirmStatusChange() {
        const result = this.dueTimeMode === OPTION_IMMEDIATELY ? null : this.dueTime;

        this.dueTimeResult?.next(result);
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

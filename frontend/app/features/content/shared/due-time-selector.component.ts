/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { DialogModel } from '@app/shared';

@Component({
    selector: 'sqx-due-time-selector',
    styleUrls: ['./due-time-selector.component.scss'],
    templateUrl: './due-time-selector.component.html'
})
export class DueTimeSelectorComponent {
    private dueTimeResult: Subject<string | null>;

    public dueTimeDialog = new DialogModel();
    public dueTime: string | null = '';
    public dueTimeAction: string | null = '';
    public dueTimeMode = 'Immediately';

    public selectDueTime(action: string): Observable<string | null> {
        this.dueTimeAction = action;
        this.dueTimeResult = new Subject<string | null>();
        this.dueTimeDialog.show();

        return this.dueTimeResult;
    }

    public confirmStatusChange() {
        const result = this.dueTimeMode === 'Immediately' ? null : this.dueTime;

        this.dueTimeResult.next(result);
        this.dueTimeResult.complete();

        this.cancelStatusChange();
    }

    public cancelStatusChange() {
        this.dueTimeMode = 'Immediately';
        this.dueTimeDialog.hide();
        this.dueTimeResult = null!;
        this.dueTime = null;
    }
}
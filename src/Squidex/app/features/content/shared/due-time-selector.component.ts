/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { DialogModel, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-due-time-selector',
    styleUrls: ['./due-time-selector.component.scss'],
    templateUrl: './due-time-selector.component.html',
    animations: [
        fadeAnimation
    ]
})
export class DueTimeSelectorComponent {
    public dueTimeDialog = new DialogModel();
    public dueTime: string | null = '';
    public dueTimeFunction: Subject<string | null>;
    public dueTimeAction: string | null = '';
    public dueTimeMode = 'Immediately';

    public selectDueTime(action: string): Observable<string | null> {
        this.dueTimeAction = action;
        this.dueTimeFunction = new Subject<string | null>();
        this.dueTimeDialog.show();

        return this.dueTimeFunction;
    }

    public confirmStatusChange() {
        const result = this.dueTimeMode === 'Immediately' ? null : this.dueTime;

        this.dueTimeFunction.next(result);
        this.dueTimeFunction.complete();

        this.cancelStatusChange();
    }

    public cancelStatusChange() {
        this.dueTimeMode = 'Immediately';
        this.dueTimeDialog.hide();
        this.dueTimeFunction = null!;
        this.dueTime = null;
    }
}


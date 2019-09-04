/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { empty, of } from 'rxjs';
import { catchError, mergeMap, tap } from 'rxjs/operators';

import {
    ContributorsState,
    ErrorDto,
    ImportContributorsForm
} from '@app/shared';

interface ImportStatus {
    email: string;
    result: 'Pending' | 'Failed' | 'Success';
    resultText: string;
}

@Component({
    selector: 'sqx-import-contributors-dialog',
    styleUrls: ['./import-contributors-dialog.component.scss'],
    templateUrl: './import-contributors-dialog.component.html'
})
export class ImportContributorsDialogComponent {
    @Output()
    public close = new EventEmitter();

    public importForm = new ImportContributorsForm(this.formBuilder);
    public importStatus: ImportStatus[];

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly contributorsState: ContributorsState
    ) {
    }

    public import() {
        const contributors = this.importForm.submit();

        if (contributors && contributors.length > 0) {
            this.importStatus = [];

            for (let contributor of contributors) {
                this.importStatus.push({
                    email: contributor.contributorId,
                    result: 'Pending',
                    resultText: 'Pending'
                });
            }

            of(...contributors).pipe(
                mergeMap(c =>
                    this.contributorsState.assign(c, { silent: true }).pipe(
                        tap(created => {
                            let status = this.importStatus.find(x => x.email === c.contributorId);

                            if (status) {
                                status.resultText = getSuccess(created);
                                status.result = 'Success';
                            }
                        }),
                        catchError((error: ErrorDto) => {
                            let status = this.importStatus.find(x => x.email === c.contributorId);

                            if (status) {
                                status.resultText = getError(error);
                                status.result = 'Failed';
                            }

                            return empty();
                        })
                    ), 1)
            ).subscribe();
        }
    }

    public emitClose() {
        this.close.emit();
    }
}

function getError(error: ErrorDto): string {
    return error.details[0];
}

function getSuccess(created: boolean | undefined): string {
    return created ?
        'User has been invited and assigned.' :
        'User has been assigned';
}

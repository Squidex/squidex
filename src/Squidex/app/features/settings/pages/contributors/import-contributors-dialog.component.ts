/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { empty, of } from 'rxjs';
import { catchError, mergeMap, tap } from 'rxjs/operators';

import {
    ContributorsState,
    ErrorDto,
    ImmutableArray,
    ImportContributorsForm,
    RoleDto
} from '@app/shared';

interface ImportStatus {
    email: string;
    result: 'Pending' | 'Failed' | 'Success';
    resultText: string;
    role: string;
}

@Component({
    selector: 'sqx-import-contributors-dialog',
    styleUrls: ['./import-contributors-dialog.component.scss'],
    templateUrl: './import-contributors-dialog.component.html'
})
export class ImportContributorsDialogComponent {
    public readonly standalone = { standalone: true };

    @Output()
    public close = new EventEmitter();

    @Input()
    public roles: ImmutableArray<RoleDto>;

    public importForm = new ImportContributorsForm(this.formBuilder);
    public importStatus: ImportStatus[] = [];
    public importStage: 'Start' | 'Change' | 'Wait' = 'Start';

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly contributorsState: ContributorsState
    ) {
    }

    public detect() {
        this.importStage = 'Change';

        const contributors = this.importForm.submit();

        if (contributors && contributors.length > 0) {
            for (let contributor of contributors) {
                this.importStatus.push({
                    email: contributor.contributorId,
                    result: 'Pending',
                    resultText: 'Pending',
                    role: 'Developer'
                });
            }
        }
    }

    public import() {
        this.importStage = 'Wait';

        of(...this.importStatus).pipe(
            mergeMap(s =>
                this.contributorsState.assign(createRequest(s), { silent: true }).pipe(
                    tap(created => {
                        let status = this.importStatus.find(x => x.email === s.email);

                        if (status) {
                            status.resultText = getSuccess(created);
                            status.result = 'Success';
                        }
                    }),
                    catchError((error: ErrorDto) => {
                        let status = this.importStatus.find(x => x.email === s.email);

                        if (status) {
                            status.resultText = getError(error);
                            status.result = 'Failed';
                        }

                        return empty();
                    })
                ), 1)
        ).subscribe();
    }

    public emitClose() {
        this.close.emit();
    }
}

function createRequest(status: ImportStatus) {
    return { contributorId: status.email, role: status.role, invite: true };
}

function getError(error: ErrorDto): string {
    return error.details[0];
}

function getSuccess(created: boolean | undefined): string {
    return created ?
        'User has been invited and assigned.' :
        'User has been assigned';
}

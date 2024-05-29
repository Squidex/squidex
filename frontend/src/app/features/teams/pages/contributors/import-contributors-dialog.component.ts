/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EMPTY, of } from 'rxjs';
import { catchError, mergeMap, tap } from 'rxjs/operators';
import { ErrorDto, FormHintComponent, ImportContributorsForm, ModalDialogComponent, StatusIconComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';

type ImportStatus = {
    email: string;
    result: 'Pending' | 'Failed' | 'Success';
    resultText: string;
    role: string;
};

@Component({
    standalone: true,
    selector: 'sqx-import-contributors-dialog',
    styleUrls: ['./import-contributors-dialog.component.scss'],
    templateUrl: './import-contributors-dialog.component.html',
    imports: [
        AsyncPipe,
        FormHintComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        StatusIconComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ImportContributorsDialogComponent {
    @Output()
    public dialogClose = new EventEmitter();

    public importForm = new ImportContributorsForm();
    public importStatus: ReadonlyArray<ImportStatus> = [];
    public importStage: 'Start' | 'Change' | 'Wait' = 'Start';

    constructor(
        private readonly contributorsState: TeamContributorsState,
    ) {
    }

    public detect() {
        this.importStage = 'Change';

        const contributors = this.importForm.submit();

        if (contributors) {
            this.importStatus = contributors.map(contributor => ({
                email: contributor.contributorId,
                result: 'Pending',
                resultText: 'Pending',
                role: 'Developer',
            }));
        }
    }

    public import() {
        this.importStage = 'Wait';

        of(...this.importStatus).pipe(
            mergeMap(s =>
                this.contributorsState.assign(createRequest(s), { silent: true }).pipe(
                    tap(created => {
                        const status = this.importStatus.find(x => x.email === s.email);

                        if (status) {
                            status.resultText = getSuccess(created);
                            status.result = 'Success';
                        }
                    }),
                    catchError((error: ErrorDto) => {
                        const status = this.importStatus.find(x => x.email === s.email);

                        if (status) {
                            status.resultText = getError(error);
                            status.result = 'Failed';
                        }

                        return EMPTY;
                    }),
                ), 1),
        ).subscribe();
    }
}

function createRequest(status: ImportStatus) {
    return { contributorId: status.email, role: status.role, invite: true };
}

function getError(error: ErrorDto): string {
    return error.details[0].originalMessage;
}

function getSuccess(created: boolean | undefined): string {
    return created ?
        'i18n:contributors.contributorAssignedInvited' :
        'i18n:contributors.contributorAssignedExisting';
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { EMPTY, of } from 'rxjs';
import { catchError, mergeMap, tap } from 'rxjs/operators';
import { AssignContributorDto, ErrorDto, FormHintComponent, ImportContributorsForm, ModalDialogComponent, StatusIconComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';

type ImportStatus = {
    email: string;
    result: 'Pending' | 'Failed' | 'Success';
    resultText: string;
    role: string;
};

@Component({
    selector: 'sqx-import-contributors-dialog',
    styleUrls: ['./import-contributors-dialog.component.scss'],
    templateUrl: './import-contributors-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormHintComponent,
        FormsModule,
        ModalDialogComponent,
        StatusIconComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ImportContributorsDialogComponent {
    private readonly contributorsState = inject(TeamContributorsState);

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    protected readonly importStatus = signal<ReadonlyArray<ImportStatus>>([]);

    protected readonly importStage = signal<'Start' | 'Change' | 'Wait'>('Start');

    protected readonly importForm = new ImportContributorsForm();

    protected detect() {
        this.importStage.set('Change');

        this.importForm.submit(async contributors => {
            this.importStatus.set(contributors.map(contributor => ({
                email: contributor.contributorId,
                result: 'Pending',
                resultText: 'Pending',
                role: 'Developer',
            })));
        });
    }

    protected import() {
        this.importStage.set('Wait');

        of(...this.importStatus()).pipe(
            mergeMap(s =>
                this.contributorsState.assign(new AssignContributorDto({ contributorId: s.email, role: s.role, invite: true }), { silent: true }).pipe(
                    tap(created => {
                        this.updateStatus(s.email, 'Success', created ? 'i18n:contributors.contributorAssignedInvited' : 'i18n:contributors.contributorAssignedExisting');
                    }),
                    catchError((error: ErrorDto) => {
                        this.updateStatus(s.email, 'Failed', error.details[0].originalMessage);

                        return EMPTY;
                    }),
                ), 1),
        ).subscribe();
    }

    private updateStatus(email: string, result: ImportStatus['result'], resultText: string) {
        this.importStatus.update(statuses => statuses.map(x => x.email === email ? { ...x, result, resultText } : x));
    }
}

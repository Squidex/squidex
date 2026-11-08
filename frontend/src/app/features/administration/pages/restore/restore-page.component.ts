/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { catchError, EMPTY, tap, timer } from 'rxjs';
import { ControlErrorsComponent, DialogService, ISODatePipe, JobsService, LayoutComponent, ListViewComponent, RestoreForm, SidebarMenuDirective, switchSafe, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormField,
        FormsModule,
        ISODatePipe,
        LayoutComponent,
        ListViewComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class RestorePageComponent {
    private readonly dialogs = inject(DialogService);
    private readonly jobsService = inject(JobsService);

    protected readonly restoreJob = rxResource({
        stream: () => timer(0, 2000).pipe(switchSafe(() => this.jobsService.getRestore())),
    });

    protected readonly restoreForm = new RestoreForm();

    protected restore() {
        this.restoreForm.submit(value => {
            return this.jobsService.postRestore(value).pipe(
                tap(() => {
                    this.dialogs.notifyInfo('i18n:jobs.restoreStarted');
                }),
                catchError(error => {
                    this.dialogs.notifyError(error);
                    return EMPTY;
                }));
        }, { resetAfterSubmit: true });
    }
}

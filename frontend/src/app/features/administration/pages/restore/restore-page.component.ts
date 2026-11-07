/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { ControlErrorsComponent, DialogService, ISODatePipe, JobsService, LayoutComponent, ListViewComponent, RestoreForm, SidebarMenuDirective, switchSafe, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html',
    imports: [
        AsyncPipe,
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

    protected readonly restoreForm = new RestoreForm();

    protected readonly restoreJob =
        timer(0, 2000).pipe(switchSafe(() => this.jobsService.getRestore()));

    protected restore() {
        const value = this.restoreForm.submit();
        if (!value) {
            return;
        }

        this.restoreForm.submitCompleted();

        this.jobsService.postRestore(value)
            .subscribe({
                next: () => {
                    this.dialogs.notifyInfo('i18n:jobs.restoreStarted');
                },
                error: error => {
                    this.dialogs.notifyError(error);
                },
            });
    }
}

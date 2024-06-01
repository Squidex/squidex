/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { AuthService, ControlErrorsComponent, DialogService, ISODatePipe, JobsService, LayoutComponent, ListViewComponent, RestoreForm, SidebarMenuDirective, switchSafe, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html',
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FormsModule,
        ISODatePipe,
        LayoutComponent,
        ListViewComponent,
        ReactiveFormsModule,
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
    public restoreForm = new RestoreForm();

    public restoreJob =
        timer(0, 2000).pipe(switchSafe(() => this.jobsService.getRestore()));

    constructor(
        public readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly jobsService: JobsService,
    ) {
    }

    public restore() {
        const value = this.restoreForm.submit();

        if (value) {
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
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { AuthService, BackupsService, ControlErrorsComponent, DialogService, ISODatePipe, LayoutComponent, ListViewComponent, RestoreForm, SidebarMenuDirective, switchSafe, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        ListViewComponent,
        NgIf,
        NgFor,
        FormsModule,
        ReactiveFormsModule,
        ControlErrorsComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TooltipDirective,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        ISODatePipe,
        TranslatePipe,
    ],
})
export class RestorePageComponent {
    public restoreForm = new RestoreForm();

    public restoreJob =
        timer(0, 2000).pipe(switchSafe(() => this.backupsService.getRestore()));

    constructor(
        public readonly authState: AuthService,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService,
    ) {
    }

    public restore() {
        const value = this.restoreForm.submit();

        if (value) {
            this.restoreForm.submitCompleted();

            this.backupsService.postRestore(value)
                .subscribe({
                    next: () => {
                        this.dialogs.notifyInfo('i18n:backups.restoreStarted');
                    },
                    error: error => {
                        this.dialogs.notifyError(error);
                    },
                });
        }
    }
}

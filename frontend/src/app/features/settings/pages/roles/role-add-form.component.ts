/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddRoleForm, ControlErrorsComponent, FormHintComponent, RolesState, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-role-add-form',
    styleUrls: ['./role-add-form.component.scss'],
    templateUrl: './role-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class RoleAddFormComponent {
    public addRoleForm = new AddRoleForm();

    constructor(
        private readonly rolesState: RolesState,
    ) {
    }

    public addRole() {
        const value = this.addRoleForm.submit();

        if (value) {
            this.rolesState.add(value)
                .subscribe({
                    next: () => {
                        this.addRoleForm.submitCompleted();
                    },
                    error: error => {
                        this.addRoleForm.submitFailed(error);
                    },
                });
        }
    }

    public cancel() {
        this.addRoleForm.submitCompleted();
    }
}

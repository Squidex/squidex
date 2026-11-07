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
    protected addRoleForm = new AddRoleForm();

    constructor(
        private readonly rolesState: RolesState,
    ) {
    }

    protected addRole() {
        const value = this.addRoleForm.submit();
        if (!value) {
            return;
        }

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

    protected cancel() {
        this.addRoleForm.submitCompleted();
    }
}

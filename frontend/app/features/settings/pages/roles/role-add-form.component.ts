/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AddRoleForm, RolesState } from '@app/shared';

@Component({
    selector: 'sqx-role-add-form',
    styleUrls: ['./role-add-form.component.scss'],
    templateUrl: './role-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleAddFormComponent {
    public addRoleForm = new AddRoleForm(this.formBuilder);

    constructor(
        private readonly rolesState: RolesState,
        private readonly formBuilder: FormBuilder,
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

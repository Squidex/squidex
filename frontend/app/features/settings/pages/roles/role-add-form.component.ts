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
    template: `
        <div class="table-items-footer">
            <form [formGroup]="addRoleForm.form" (ngSubmit)="addRole()">
                <div class="row no-gutters">
                    <div class="col">
                        <sqx-control-errors for="name" [submitted]="addRoleForm.submitted | async"></sqx-control-errors>

                        <input type="text" class="form-control" formControlName="name" maxlength="40" placeholder="Enter role name" autocomplete="off" />
                    </div>
                    <div class="col-auto pl-1">
                        <button type="submit" class="btn btn-success" [disabled]="addRoleForm.hasNoName | async">Add role</button>
                    </div>
                    <div class="col-auto pl-1">
                        <button type="reset" class="btn btn-text-secondary2" (click)="cancel()">Cancel</button>
                    </div>
                </div>
            </form>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoleAddFormComponent {
    public addRoleForm = new AddRoleForm(this.formBuilder);

    constructor(
        private readonly rolesState: RolesState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public addRole() {
        const value = this.addRoleForm.submit();

        if (value) {
            this.rolesState.add(value)
                .subscribe(() => {
                    this.addRoleForm.submitCompleted();
                }, error => {
                    this.addRoleForm.submitFailed(error);
                });
        }
    }

    public cancel() {
        this.addRoleForm.submitCompleted();
    }
}
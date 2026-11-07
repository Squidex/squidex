/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AddRoleForm, ControlErrorsComponent, FormHintComponent, RolesState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-role-add-form',
    styleUrls: ['./role-add-form.component.scss'],
    templateUrl: './role-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormField,
        FormHintComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class RoleAddFormComponent {
    private readonly rolesState = inject(RolesState);

    protected readonly addRoleForm = new AddRoleForm();

    protected addRole() {
        this.addRoleForm.submit(value => {
            return this.rolesState.add(value);
        }, { resetAfterSubmit: true });
    }

    protected cancel() {
        this.addRoleForm.reset();
    }
}

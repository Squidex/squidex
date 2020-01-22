/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { AddClientForm, ClientsState } from '@app/shared';

@Component({
    selector: 'sqx-client-add-form',
    template: `
        <div class="table-items-footer">
            <form [formGroup]="addClientForm.form" (ngSubmit)="addClient()">
                <div class="row no-gutters">
                    <div class="col">
                        <sqx-control-errors for="name" [submitted]="addClientForm.submitted | async"></sqx-control-errors>

                        <input type="text" class="form-control" formControlName="id" maxlength="40" placeholder="Enter client name" autocomplete="off" sqxTransformInput="LowerCase" />
                    </div>
                    <div class="col-auto pl-1">
                        <button type="submit" class="btn btn-success" [disabled]="addClientForm.hasNoId | async">Add Client</button>
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
export class ClientAddFormComponent {
    public addClientForm = new AddClientForm(this.formBuilder);

    constructor(
        private readonly clientsState: ClientsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public addClient() {
        const value = this.addClientForm.submit();

        if (value) {
            this.clientsState.attach(value)
                .subscribe(() => {
                    this.addClientForm.submitCompleted();
                }, error => {
                    this.addClientForm.submitFailed(error);
                });
        }
    }

    public cancel() {
        this.addClientForm.submitCompleted();
    }
}
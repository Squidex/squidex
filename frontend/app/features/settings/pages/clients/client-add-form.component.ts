/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AddClientForm, ClientsState } from '@app/shared';

@Component({
    selector: 'sqx-client-add-form',
    styleUrls: ['./client-add-form.component.scss'],
    templateUrl: './client-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClientAddFormComponent {
    public addClientForm = new AddClientForm();

    constructor(
        private readonly clientsState: ClientsState,
    ) {
    }

    public addClient() {
        const value = this.addClientForm.submit();

        if (value) {
            this.clientsState.attach(value)
                .subscribe({
                    next: () => {
                        this.addClientForm.submitCompleted();
                    },
                    error: error => {
                        this.addClientForm.submitFailed(error);
                    },
                });
        }
    }

    public cancel() {
        this.addClientForm.submitCompleted();
    }
}

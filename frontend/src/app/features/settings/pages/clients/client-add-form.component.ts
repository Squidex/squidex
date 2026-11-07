/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddClientForm, ClientsState, ControlErrorsComponent, FormHintComponent, TransformInputDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-client-add-form',
    styleUrls: ['./client-add-form.component.scss'],
    templateUrl: './client-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class ClientAddFormComponent {
    private readonly clientsState = inject(ClientsState);

    protected addClientForm = new AddClientForm();

    protected addClient() {
        const value = this.addClientForm.submit();
        if (!value) {
            return;
        }

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

    protected cancel() {
        this.addClientForm.submitCompleted();
    }
}

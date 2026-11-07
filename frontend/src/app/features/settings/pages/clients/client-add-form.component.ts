/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AddClientForm, ClientsState, ControlErrorsComponent, FormHintComponent, TransformInputDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-client-add-form',
    styleUrls: ['./client-add-form.component.scss'],
    templateUrl: './client-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormField,
        FormHintComponent,
        FormsModule,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class ClientAddFormComponent {
    private readonly clientsState = inject(ClientsState);

    protected readonly addClientForm = new AddClientForm();

    protected addClient() {
        this.addClientForm.submit(value => {
            return this.clientsState.attach(value);
        }, { resetAfterSubmit: true });
    }

    protected cancel() {
        this.addClientForm.reset();
    }
}

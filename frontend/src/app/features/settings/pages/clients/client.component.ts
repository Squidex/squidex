/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, inject, input, linkedSignal } from '@angular/core';
import { disabled, form, FormField, min } from '@angular/forms/signals';
import { AppsState, ClientDto, ClientsState, ConfirmClickDirective, CopyDirective, DialogModel, EditableTitleComponent, FormRowComponent, ModalDirective, RoleDto, TooltipDirective, TourStepDirective, TranslatePipe, UpdateClientDto } from '@app/shared';
import { ClientConnectFormComponent } from './client-connect-form.component';

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html',
    imports: [
        ClientConnectFormComponent,
        ConfirmClickDirective,
        CopyDirective,
        EditableTitleComponent,
        FormField,
        FormRowComponent,
        ModalDirective,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class ClientComponent {
    private readonly clientsState = inject(ClientsState);

    protected readonly appsState = inject(AppsState);

    /** The client to show and edit. */
    public readonly client = input.required<ClientDto>();

    /** The roles that can be assigned to the client. */
    public readonly clientRoles = input.required<ReadonlyArray<RoleDto>>();

    protected readonly apiCallsLimit = form(linkedSignal(() => this.client().apiCallsLimit), path => {
        disabled(path,
            {when: () => !this.client().canUpdate }
        );
        min(path, 0);
    });

    protected readonly connectDialog = new DialogModel(false);

    protected revoke() {
        this.clientsState.revoke(this.client());
    }

    protected regenerateSecret() {
        this.clientsState.regenerateSecret(this.client());
    }

    protected updateRole(role: string) {
        const request = new UpdateClientDto({ role });

        this.clientsState.update(this.client(), request);
    }

    protected updateAllowAnonymous(allowAnonymous: boolean) {
        const request = new UpdateClientDto({ allowAnonymous });

        this.clientsState.update(this.client(), request);
    }

    protected updateApiCallsLimit(apiCallsLimit: number) {
        const request = new UpdateClientDto({ apiCallsLimit });

        this.clientsState.update(this.client(), request);
    }

    protected rename(name: string) {
        const request = new UpdateClientDto({ name });

        this.clientsState.update(this.client(), request);
    }
}

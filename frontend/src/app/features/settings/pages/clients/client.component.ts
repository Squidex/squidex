/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppsState, ClientDto, ClientsState, ConfirmClickDirective, CopyDirective, DialogModel, EditableTitleComponent, FormRowComponent, ModalDirective, RoleDto, TooltipDirective, TourStepDirective, TranslatePipe, UpdateClientDto } from '@app/shared';
import { ClientConnectFormComponent } from './client-connect-form.component';

@Component({
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ClientConnectFormComponent,
        ConfirmClickDirective,
        CopyDirective,
        EditableTitleComponent,
        FormRowComponent,
        FormsModule,
        ModalDirective,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class ClientComponent {
    /** The client to show and edit. */
    public readonly client = input.required<ClientDto>();

    /** The roles that can be assigned to the client. */
    public readonly clientRoles = input.required<ReadonlyArray<RoleDto>>();

    protected readonly apiCallsLimit = linkedSignal(() => this.client().apiCallsLimit);

    protected readonly connectDialog = new DialogModel(false);

    constructor(
        protected readonly appsState: AppsState,
        private readonly clientsState: ClientsState,
    ) {
    }

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

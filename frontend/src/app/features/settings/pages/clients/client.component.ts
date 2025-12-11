/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppsState, ClientDto, ClientsState, ConfirmClickDirective, CopyDirective, DialogModel, EditableTitleComponent, FormRowComponent, ModalDirective, RoleDto, TooltipDirective, TourStepDirective, TranslatePipe, TypedSimpleChanges, UpdateClientDto } from '@app/shared';
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
    @Input({ required: true })
    public client!: ClientDto;

    @Input({ required: true })
    public clientRoles!: ReadonlyArray<RoleDto>;

    public apiCallsLimit = 0;

    public connectDialog = new DialogModel(false);

    constructor(
        public readonly appsState: AppsState,
        private readonly clientsState: ClientsState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.client) {
            this.apiCallsLimit = this.client.apiCallsLimit;
        }
    }

    public revoke() {
        this.clientsState.revoke(this.client);
    }

    public updateRole(role: string) {
        const request = new UpdateClientDto({ role });

        this.clientsState.update(this.client, request);
    }

    public updateAllowAnonymous(allowAnonymous: boolean) {
        const request = new UpdateClientDto({ allowAnonymous });

        this.clientsState.update(this.client, request);
    }

    public updateApiCallsLimit(apiCallsLimit: number) {
        const request = new UpdateClientDto({ apiCallsLimit });

        this.clientsState.update(this.client, request);
    }

    public rename(name: string) {
        const request = new UpdateClientDto({ name });

        this.clientsState.update(this.client, request);
    }
}
